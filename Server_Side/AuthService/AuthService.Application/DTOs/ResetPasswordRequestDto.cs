using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.DTOs
{
    public sealed class ResetPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(500)]
        public string ResetToken { get; set; } = null!;

        [Required]
        [StringLength(256, MinimumLength = 6)]
        public string NewPassword { get; set; } = null!;

        [Required]
        [Compare(nameof(NewPassword))]
        public string ConfirmNewPassword { get; set; } = null!;
    }
}