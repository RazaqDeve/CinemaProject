
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using IdentityService.Service.OtpRedis.SmsOtp;
using IdentityService.Service.SmsOtp;
using Microsoft.Extensions.Options;

namespace IdentityService.Service.MeliPayamak;

public class MeliPayamakSender : ISmsSender
{
    private readonly HttpClient _httpClient;
    private readonly MeliPayamakOptions _options;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

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
        // 1. Validate phone number
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new ArgumentException(
                "Phone number is required.",
                nameof(phoneNumber));
        }

        // 2. Validate OTP
        if (!Regex.IsMatch(code, @"^[0-9]{6}$"))
        {
            throw new ArgumentException(
                "OTP must contain exactly 6 digits.",
                nameof(code));
        }

        // 3. Validate configuration
        if (string.IsNullOrWhiteSpace(_options.Username) ||
            string.IsNullOrWhiteSpace(_options.Password) ||
            _options.BodyId <= 0)
        {
            throw new InvalidOperationException(
                "MeliPayamak configuration is incomplete.");
        }

        // 4. Prepare API parameters
        var parameters = new Dictionary<string, string>
        {
            ["username"] = _options.Username,
            ["password"] = _options.Password,
            ["to"] = phoneNumber.Trim(),
            ["text"] = code,
            ["bodyId"] = _options.BodyId.ToString(CultureInfo.InvariantCulture)
        };

        using var content = new FormUrlEncodedContent(parameters);

        // 5. Send HTTP request
        using var response = await _httpClient.PostAsync(
            "api/SendSMS/BaseServiceNumber",
            content,
            cancellationToken);

        // 6. Read response
        string responseBody = await response.Content.ReadAsStringAsync(
            cancellationToken);

        // 7. Check HTTP status
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"MeliPayamak HTTP error: {(int)response.StatusCode}",
                null,
                response.StatusCode);
        }

        // 8. Parse MeliPayamak response
        MeliPayamakResponse? result;

        try
        {
            result = JsonSerializer.Deserialize<MeliPayamakResponse>(
                responseBody,
                JsonOptions);
        }
        catch (JsonException exception)
        {
            throw new HttpRequestException(
                "MeliPayamak returned an invalid response.",
                exception);
        }

        if (result is null)
        {
            throw new HttpRequestException(
                "MeliPayamak returned an empty response.");
        }

        // 9. Check API-level result
        if (result.RetStatus != 1 ||
            string.IsNullOrWhiteSpace(result.Value))
        {
            throw new HttpRequestException(
                $"MeliPayamak rejected SMS request. " +
                $"RetStatus: {result.RetStatus}, " +
                $"Message: {result.StrRetStatus}");
        }

        // 10. SMS request accepted by provider
    }

    private sealed class MeliPayamakResponse
    {
        public string? Value { get; set; }

        public int RetStatus { get; set; }

        public string? StrRetStatus { get; set; }
    }
}
