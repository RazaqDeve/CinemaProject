using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using IdentityService.Service.OtpRedis.SmsOtp;
using Microsoft.Extensions.Options;

namespace IdentityService.Service.Sms;

public class SmsIrSender : ISmsSender
{
    private readonly HttpClient _httpClient;
    private readonly SmsIrOptions _options;

    public SmsIrSender(
        HttpClient httpClient,
        IOptions<SmsIrOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "SMS.ir API Key is not configured.");
        }

        if (_options.TemplateId <= 0)
        {
            throw new InvalidOperationException(
                "SMS.ir TemplateId is not configured.");
        }

        //  _httpClient.DefaultRequestHeaders.Add(
        //     "X-API-KEY",
        //    _options.ApiKey);
    }

    public async Task SendOtpAsync(
        string phoneNumber,
        string code,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);

        if (!Regex.IsMatch(code, @"^[0-9]{6}$"))
        {
            throw new ArgumentException(
                "OTP must contain exactly 6 digits.",
                nameof(code));
        }

        string mobile = NormalizeIranianMobile(phoneNumber);

        var request = new VerifySendRequest
        {
            Mobile = mobile,
            TemplateId = _options.TemplateId,
            Parameters =
            [
                new VerifySendParameter
                {
                    Name = _options.ParameterName,
                    Value = code
                }
            ]
        };

        using HttpResponseMessage response =
            await _httpClient.PostAsJsonAsync(
                "v1/send/verify",
                request,
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"SMS.ir returned HTTP {(int)response.StatusCode}.",
                null,
                response.StatusCode);
        }

        var apiResult =
            await response.Content.ReadFromJsonAsync<SmsIrResponse>(
                cancellationToken: cancellationToken);

        if (apiResult is null || apiResult.Status != 1)
        {
            throw new HttpRequestException(
                $"SMS.ir rejected the request: " +
                $"{apiResult?.Message ?? "Unknown provider error"}");
        }
    }

    private static string NormalizeIranianMobile(string phoneNumber)
    {
        string mobile = phoneNumber.Trim()
            .Replace(" ", "")
            .Replace("-", "");

        if (mobile.StartsWith("+98"))
        {
            mobile = mobile[3..];
        }
        else if (mobile.StartsWith("0098"))
        {
            mobile = mobile[4..];
        }
        else if (mobile.StartsWith("98") && mobile.Length == 12)
        {
            mobile = mobile[2..];
        }

        if (mobile.StartsWith("0") && mobile.Length == 11)
        {
            mobile = mobile[1..];
        }

        if (mobile.Length != 10 ||
            mobile[0] != '9' ||
            mobile.Any(c => c < '0' || c > '9'))
        {
            throw new ArgumentException(
                "Invalid Iranian mobile number.",
                nameof(phoneNumber));
        }

        return mobile;
    }

    private sealed class VerifySendRequest
    {
        [JsonPropertyName("mobile")]
        public string Mobile { get; set; } = string.Empty;

        [JsonPropertyName("templateId")]
        public int TemplateId { get; set; }

        [JsonPropertyName("parameters")]
        public VerifySendParameter[] Parameters { get; set; } = [];
    }

    private sealed class VerifySendParameter
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }

    private sealed class SmsIrResponse
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
