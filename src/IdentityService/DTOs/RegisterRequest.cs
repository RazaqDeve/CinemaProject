using System.ComponentModel.DataAnnotations;

namespace IdentityService.DTOs
{
    public class RegisterRequest
    {
        
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;
    }
}