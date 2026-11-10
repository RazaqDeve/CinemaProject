

namespace IdentityService.Service.OtpRedis.SmsOtp
{
    public class SmsIrOptions
    {
        public string ApiKey { get; set; } = string.Empty;

        public int TemplateId { get; set; }

        public string ParameterName { get; set; } = "CODE";
    }
}