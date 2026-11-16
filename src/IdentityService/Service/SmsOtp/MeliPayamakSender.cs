using System.Net.Http;
using System.Text.RegularExpressions;
using IdentityService.Service.OtpRedis.SmsOtp;
using IdentityService.Service.SmsOtp;
using Microsoft.Extensions.Options;

namespace IdentityService.Service.MeliPayamak;

public class MeliPayamakSender : ISmsSender
{
    private readonly HttpClient _httpClient;
    private readonly MeliPayamakOptions _options;

    public MeliPayamakSender(
        HttpClient httpClient,
        IOptions<MeliPayamakOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task SendOtpAsync(
        string phoneNumber,
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number is required.");

        if (!Regex.IsMatch(code, @"^[0-9]{6}$"))
            throw new ArgumentException("OTP must contain exactly 6 digits.");

        var parameters = new Dictionary<string, string>
        {
            ["username"] = _options.Username,
            ["password"] = _options.Password,
            ["to"] = phoneNumber,
            ["from"] = _options.From,
            ["code"] = code
        };

        using var content = new FormUrlEncodedContent(parameters);

        using var response = await _httpClient.PostAsync(
            "api/SendSMS/SendOtp",
            content,
            cancellationToken);

        string responseBody = await response.Content.ReadAsStringAsync(
            cancellationToken);

        response.EnsureSuccessStatusCode();

        // فعلاً پاسخ خام سرویس را نگه می‌داریم
        // تا در مرحله بعد فرمت موفقیت آن را بررسی کنیم.
    }
}
