
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace IdentityService.Service.OtpRedis;

public sealed class OtpHashHelper
{
    private readonly byte[] _otpHmacKey;
    private readonly byte[] _phoneHashKey;

    public OtpHashHelper(IConfiguration configuration)
    {
        _otpHmacKey = ReadKey(configuration, "Otp:HmacKey");

        _phoneHashKey = ReadKey(configuration, "Otp:PhoneHashKey");
    }

    private static byte[] ReadKey(IConfiguration configuration, string settingName)
    {
        string? encodedKey = configuration[settingName];

        if (string.IsNullOrWhiteSpace(encodedKey))
        {
            throw new InvalidOperationException($"{settingName} is not configured.");
        }

        byte[] key;

        try
        {
            key = Convert.FromBase64String(encodedKey);
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException($"{settingName} must be a valid Base64 string.", exception);
        }

        if (key.Length < 32)
        {
            throw new InvalidOperationException($"{settingName} must contain at least 32 bytes.");
        }

        return key;
    }

    // Hash phone number with a separate secret key
    public string CreatePhoneHash(string phoneNumber)
    {
        string normalizedPhone = phoneNumber.Trim();

        byte[] payload = Encoding.UTF8.GetBytes($"phone:v1:{normalizedPhone}");
        byte[] hash = HMACSHA256.HashData(_phoneHashKey, payload);
        return Convert.ToHexString(hash);
    }

    // Hash OTP with its own secret key
    public string CreateOtpHash(string phoneHash, string code)
    {
        byte[] payload = Encoding.UTF8.GetBytes($"otp:v1:{phoneHash}:{code}");
        byte[] hash = HMACSHA256.HashData(_otpHmacKey, payload);
        return Convert.ToHexString(hash);
    }
}
