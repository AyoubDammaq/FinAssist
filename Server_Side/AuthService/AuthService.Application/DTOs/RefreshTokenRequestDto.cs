using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.DTOs
{
    public sealed class RefreshTokenRequestDto
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(500)]
        public string RefreshToken { get; set; } = null!;
    }
}
