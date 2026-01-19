using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.DTOs
{
    public sealed class ForgotPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = null!;
    }
}