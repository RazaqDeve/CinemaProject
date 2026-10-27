using System.ComponentModel.DataAnnotations;

namespace IdentityService.DTOs
{
    public class VerifyOtpRequest
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Code { get; set; } = string.Empty;
    }
}