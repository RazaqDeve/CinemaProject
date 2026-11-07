using System.Globalization;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Linq;
using Microsoft.Extensions.Configuration;



namespace IdentityService.Service;

public class OtpService : IOtpService
{
    private readonly IDatabase _database;
    private readonly byte[] _hmacKey;
    private static readonly TimeSpan OtpExpiration = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan RequestWindow = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan RequestCooldown = TimeSpan.FromSeconds(60);
    private const int MaxAttempts = 5;
    private const int MaxRequests = 3;

    public OtpService(IConnectionMultiplexer redis, IConfiguration configuration)
    {
        _database = redis.GetDatabase();

        string? encodedKey = configuration["Otp:HmacKey"];

        if (string.IsNullOrWhiteSpace(encodedKey))
        {
            throw new InvalidOperationException("OTP HMAC key is not configured.");
        }

        try
        {
            _hmacKey = Convert.FromBase64String(encodedKey);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException("OTP HMAC key must be a valid Base64 string.", exception);
        }

        if (_hmacKey.Length < 32)
        {
            throw new InvalidOperationException("OTP HMAC key must contain at least 32 bytes.");
        }
    }

    public Task<string> GenerateOtpAsync(CancellationToken token = default)
    {
        // بررسی لغو شدن عملیات
        token.ThrowIfCancellationRequested();

        int number = RandomNumberGenerator.GetInt32(0, 1_000_000);
        string code = number.ToString("D6", CultureInfo.InvariantCulture);
        return Task.FromResult(code);
    }

    public async Task<OtpSaveStatus> SaveOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // ابتدا اعتبارسنجی، سپس Trim
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        phoneNumber = phoneNumber.Trim();

        if (code.Length != 6 || code.Any(c => c < '0' || c > '9'))
        {
            throw new ArgumentException("OTP must contain exactly 6 digits.", nameof(code));
        }

        var keys = GetRedisKeys(phoneNumber);

        // Create HMAC instead of storing the original OTP
        string otpHash = CreateOtpHash(keys.PhoneHash, code);

        const string script = """
            if redis.call('EXISTS', KEYS[3]) == 1 then
                return 1
            end

            local requests = tonumber(
                redis.call('GET', KEYS[2]) or '0'
            )

            if requests >= tonumber(ARGV[2]) then
                return 2
            end

            local newCount = redis.call('INCR', KEYS[2])

            if newCount == 1 then
                redis.call('EXPIRE', KEYS[2], tonumber(ARGV[3]))
            end

            redis.call(
                'SET',
                KEYS[1],
                ARGV[1],
                'EX',
                tonumber(ARGV[4])
            )

            redis.call(
                'SET',
                KEYS[4],
                0,
                'EX',
                tonumber(ARGV[4])
            )

            redis.call(
                'SET',
                KEYS[3],
                1,
                'EX',
                tonumber(ARGV[5])
            )

            return 0
            """;

        RedisKey[] redisKeys =
        [
            keys.Otp,
            keys.Requests,
            keys.Cooldown,
            keys.Attempts
        ];

        RedisValue[] values =
        [
            otpHash,
            MaxRequests,
            (int)RequestWindow.TotalSeconds,
            (int)OtpExpiration.TotalSeconds,
            (int)RequestCooldown.TotalSeconds
        ];

        RedisResult result = await _database.ScriptEvaluateAsync(script, redisKeys, values);

        int status = (int)result;

        return status switch
        {
            0 => OtpSaveStatus.Saved,
            1 => OtpSaveStatus.CooldownActive,
            2 => OtpSaveStatus.RequestLimitReached,

            _ => throw new InvalidOperationException(
                "Unexpected Redis response.")
        };
        // ساخت اطلاعات OTP همراه با زمان انقضا
        // var otpEntry = new OtpEntry(code, DateTimeOffset.UtcNow.Add(OtpExpiration));
        //  _otpCodes[phoneNumber] = otpEntry;
        //  return Task.CompletedTask;
    }

    // 3. Verify OTP atomically
    public async Task<OtpVerificationStatus> VerifyOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return OtpVerificationStatus.ExpiredOrNotFound;
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            return OtpVerificationStatus.InvalidCode;
        }

        phoneNumber = phoneNumber.Trim();

        var keys = GetRedisKeys(phoneNumber);

        // Read the stored HMAC
        RedisValue storedValue = await _database.StringGetAsync(keys.Otp);

        if (storedValue.IsNull)
        {
            return OtpVerificationStatus.ExpiredOrNotFound;
        }

        string storedHash = storedValue.ToString();
        // Hash the submitted code
        string submittedHash = CreateOtpHash(keys.PhoneHash, code);

        byte[] storedBytes = Encoding.ASCII.GetBytes(storedHash);
        byte[] submittedBytes = Encoding.ASCII.GetBytes(submittedHash);

        // Constant-time comparison
        bool isValid = CryptographicOperations.FixedTimeEquals(storedBytes, submittedBytes);

        if (isValid)
        {
            // Consume OTP atomically
            const string successScript = """
                local current = redis.call('GET', KEYS[1])

                if not current or current ~= ARGV[1] then
                    return 0
                end

                redis.call('DEL', KEYS[1], KEYS[2])

                return 1
                """;

            RedisResult result = await _database.ScriptEvaluateAsync(
                successScript,
                [keys.Otp, keys.Attempts],
                [storedHash]);

            int status = (int)result;

            return status == 1
                ? OtpVerificationStatus.Verified
                : OtpVerificationStatus.ExpiredOrNotFound;
        }

        // Wrong OTP: increment attempts atomically
        const string failedScript = """
            local current = redis.call('GET', KEYS[1])

            if not current or current ~= ARGV[1] then
                return 0
            end

            local attempts = redis.call('INCR', KEYS[2])

            if attempts == 1 then
                redis.call('EXPIRE', KEYS[2], tonumber(ARGV[3]))
            end

            if attempts >= tonumber(ARGV[2]) then
                redis.call('DEL', KEYS[1], KEYS[2])
                return 2
            end

            return 1
            """;

        RedisResult failedResult = await _database.ScriptEvaluateAsync(
            failedScript,
            [keys.Otp, keys.Attempts],
            [
                storedHash,
                MaxAttempts,
                (int)OtpExpiration.TotalSeconds
            ]);

        int failedStatus = (int)failedResult;

        return failedStatus switch
        {
            0 => OtpVerificationStatus.ExpiredOrNotFound,
            1 => OtpVerificationStatus.InvalidCode,
            2 => OtpVerificationStatus.AttemptsExceeded,

            _ => throw new InvalidOperationException("Unexpected Redis response.")
        };
    }

    // Generate Redis keys for a phone number
    private static (string PhoneHash, RedisKey Otp, RedisKey Attempts, RedisKey Requests, RedisKey Cooldown) GetRedisKeys(string phoneNumber)
    {
        byte[] phoneBytes = Encoding.UTF8.GetBytes(phoneNumber);
        string phoneHash = Convert.ToHexString(SHA256.HashData(phoneBytes));
        string prefix = $"IdentityService:{{{phoneHash}}}";

        return (phoneHash, $"{prefix}:otp", $"{prefix}:attempts", $"{prefix}:requests", $"{prefix}:cooldown");
    }

    // HMAC-SHA256 for the OTP
    private string CreateOtpHash(string phoneHash, string code)
    {
        byte[] payload = Encoding.UTF8.GetBytes($"{phoneHash}:{code}");
        byte[] hash = HMACSHA256.HashData(_hmacKey, payload);

        return Convert.ToHexString(hash);
    }
}



