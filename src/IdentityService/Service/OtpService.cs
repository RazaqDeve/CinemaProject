using System.Globalization;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Distributed;


namespace IdentityService.Service;

public class OtpService : IOtpService
{
    // private readonly ConcurrentDictionary<string, OtpEntry> _otpCodes = new();
    private readonly IDistributedCache _cache;
    private static readonly TimeSpan OtpExpiration = TimeSpan.FromMinutes(2);
    public OtpService(IDistributedCache distributedCache) => _cache = distributedCache;

    public Task<string> GenerateOtpAsync(CancellationToken token = default)
    {
        // بررسی لغو شدن عملیات
        token.ThrowIfCancellationRequested();

        int number = RandomNumberGenerator.GetInt32(0, 1_000_000);
        string code = number.ToString("D6", CultureInfo.InvariantCulture);
        return Task.FromResult(code);
    }

    public async Task SaveOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
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

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = OtpExpiration
        };

        await _cache.SetStringAsync(GetCacheKey(phoneNumber), code, options, cancellationToken);

        // ساخت اطلاعات OTP همراه با زمان انقضا
        // var otpEntry = new OtpEntry(code, DateTimeOffset.UtcNow.Add(OtpExpiration));
        //  _otpCodes[phoneNumber] = otpEntry;
        //  return Task.CompletedTask;
    }

    public async Task<bool> VerifyOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        // قبل از جست‌وجو، شماره را یکسان‌سازی می‌کنیم
        phoneNumber = phoneNumber.Trim();

        if (code.Length != 6 || code.Any(c => c < '0' || c > '9'))
        {
            return false;
        }

        string cacheKey = GetCacheKey(phoneNumber);

        // دریافت کد ذخیره‌شده از Redis
        string? storedCode = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (storedCode is null)
        {
            return false;
        }

        // مقایسه امن کد واردشده با کد ذخیره‌شده
        bool isValid = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(code),
            Encoding.UTF8.GetBytes(storedCode));

        if (!isValid)
        {
            return false;
        }

        // حذف کد پس از تأیید موفق
        await _cache.RemoveAsync(cacheKey, cancellationToken);
        return true;
    }
    private static string GetCacheKey(string phoneNumber)
    {
        return $"otp:{phoneNumber.Trim()}";
    }
}



