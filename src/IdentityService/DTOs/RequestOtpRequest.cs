using System.ComponentModel.DataAnnotations;

namespace IdentityService.DTOs
{
    public class RequestOtpRequest
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}