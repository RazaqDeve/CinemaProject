using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using IdentityService.Service.OtpRedis;
using StackExchange.Redis;


namespace IdentityService.Service;

public class OtpService : IOtpService
{
    private readonly IDatabase _database;
    private readonly OtpHashHelper _hashHelper;

    private static readonly TimeSpan OtpExpiration = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan RequestWindow = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan RequestCooldown = TimeSpan.FromSeconds(60);
    private const int MaxAttempts = 5;
    private const int MaxRequests = 3;

    public OtpService(IConnectionMultiplexer redis, OtpHashHelper hashHelper)
    {
        _database = redis.GetDatabase();
        _hashHelper = hashHelper;
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

        if (!IsValidOtp(code))
        {
            throw new ArgumentException("OTP must contain exactly 6 digits.", nameof(code));
        }

        // Hash phone number using a separate secret key
        string phoneHash = _hashHelper.CreatePhoneHash(phoneNumber);

        // Generate Redis keys
        var keys = OtpRedisKeys.CreateFromPhoneHash(phoneHash);
        // Hash OTP
        string otpHash = _hashHelper.CreateOtpHash(keys.PhoneHash, code);
        // Generate a unique nonce for this OTP
        string nonce = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        // Store nonce and OTP hash together
        string otpRecord = $"{nonce}:{otpHash}";

        RedisKey[] redisKeys =
        [
            keys.Otp,
            keys.Requests,
            keys.Cooldown,
            keys.Attempts
        ];

        RedisValue[] values =
        [
            otpRecord,
            MaxRequests,
            (int)RequestWindow.TotalSeconds,
            (int)OtpExpiration.TotalSeconds,
            (int)RequestCooldown.TotalSeconds
        ];

        RedisResult result = await _database.ScriptEvaluateAsync(OtpRedisScripts.SaveOtp, redisKeys, values);

        int status = (int)result;

        return status switch
        {
            0 => OtpSaveStatus.Saved,
            1 => OtpSaveStatus.CooldownActive,
            2 => OtpSaveStatus.RequestLimitReached,

            _ => throw new InvalidOperationException(
                "Unexpected Redis response.")
        };
    }

    // 3. Verify OTP atomically
    public async Task<OtpVerificationStatus> VerifyOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return OtpVerificationStatus.ExpiredOrNotFound;
        }

        if (!IsValidOtp(code))
        {
            return OtpVerificationStatus.InvalidCode;
        }

        phoneNumber = phoneNumber.Trim();

        string phoneHash = _hashHelper.CreatePhoneHash(phoneNumber);
        var keys = OtpRedisKeys.CreateFromPhoneHash(phoneHash);

        // Read the stored HMAC
        RedisValue storedValue = await _database.StringGetAsync(keys.Otp);

        if (storedValue.IsNull)
        {
            return OtpVerificationStatus.ExpiredOrNotFound;
        }

        // Get the complete OTP record from Redis
        string otpRecord = storedValue.ToString();
        // Expected format: 32-character nonce + ':' + 64-character HMAC
        if (otpRecord.Length != 97 || otpRecord[32] != ':')
        {
            return OtpVerificationStatus.ExpiredOrNotFound;
        }
        // Extract the stored HMAC
        string storedHash = otpRecord[33..];

        // Hash the submitted code
        string submittedHash = _hashHelper.CreateOtpHash(keys.PhoneHash, code);

        byte[] storedBytes = Encoding.ASCII.GetBytes(storedHash);
        byte[] submittedBytes = Encoding.ASCII.GetBytes(submittedHash);

        // Constant-time comparison
        bool isValid = CryptographicOperations.FixedTimeEquals(storedBytes, submittedBytes);
        RedisResult result;

        if (isValid)
        {
            result = await _database.ScriptEvaluateAsync(
                OtpRedisScripts.ConsumeOtp,
                [keys.Otp, keys.Attempts],
                [otpRecord]);

            return (int)result == 1
                ? OtpVerificationStatus.Verified
                : OtpVerificationStatus.ExpiredOrNotFound;
        }

        result = await _database.ScriptEvaluateAsync(
            OtpRedisScripts.FailedAttempt,
            [keys.Otp, keys.Attempts],
            [
                otpRecord,
                MaxAttempts,
                (int)OtpExpiration.TotalSeconds
            ]);

        int status = (int)result;

        return status switch
        {
            0 => OtpVerificationStatus.ExpiredOrNotFound,
            1 => OtpVerificationStatus.InvalidCode,
            2 => OtpVerificationStatus.AttemptsExceeded,

            _ => throw new InvalidOperationException(
                "Unexpected Redis response.")
        };
    }

    // Validate six-digit OTP
    private static bool IsValidOtp(string? code)
    {
        return code is { Length: 6 } && code.All(c => c >= '0' && c <= '9');
    }
}



