using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.DTOs
{
    public sealed class ChangePasswordRequestDto
    {
        [Required]
        [StringLength(256, MinimumLength = 6)]
        public string CurrentPassword { get; set; } = null!;

        [Required]
        [StringLength(256, MinimumLength = 6)]
        public string NewPassword { get; set; } = null!;

        [Required]
        [Compare(nameof(NewPassword))]
        public string ConfirmNewPassword { get; set; } = null!;
    }
}