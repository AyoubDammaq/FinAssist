using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(256, MinimumLength = 6)]
        public string Password { get; set; } = null!;
    }
}
