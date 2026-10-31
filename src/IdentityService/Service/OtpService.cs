using System.Text;
using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;


namespace IdentityService.Service;

public class OtpService : IOtpService
{
    private readonly ConcurrentDictionary<string, OtpEntry> _otpCodes = new();
    private static readonly TimeSpan OtpExpiration = TimeSpan.FromMinutes(2);

    public Task<string> GenerateOtpAsync(CancellationToken token = default)
    {
        int number = RandomNumberGenerator.GetInt32(0, 1_000_000);
        string code = number.ToString("D6", CultureInfo.InvariantCulture);
        return Task.FromResult(code);
    }

    public Task SaveOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        phoneNumber = phoneNumber.Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        var otpEntry = new OtpEntry(code, DateTimeOffset.UtcNow.Add(OtpExpiration));

        _otpCodes[phoneNumber] = otpEntry;
        return Task.CompletedTask;
    }

    public Task<bool> VerifyOtpAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(code))
        {
            return Task.FromResult(false);
        }

        if (!_otpCodes.TryGetValue(phoneNumber, out var otpEntry))
        {
            return Task.FromResult(false);
        }

        if (DateTimeOffset.UtcNow >= otpEntry.ExpiresAt)
        {
            _otpCodes.TryRemove(phoneNumber, out _);
            return Task.FromResult(false);
        }

        if (code.Length != 6 || code.Any(c => c < '0' || c > '9'))
        {
            return Task.FromResult(false);
        }

        phoneNumber = phoneNumber.Trim();
        bool isValid = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(code),
            Encoding.UTF8.GetBytes(otpEntry.Code));

        if (!isValid)
        {
            return Task.FromResult(false);
        }

        _otpCodes.TryRemove(phoneNumber, out _);
        return Task.FromResult(true);
    }
    private sealed record OtpEntry(string Code, DateTimeOffset ExpiresAt);
}



