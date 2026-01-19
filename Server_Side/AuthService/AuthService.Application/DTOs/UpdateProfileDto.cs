using System.ComponentModel.DataAnnotations;

namespace AuthService.Application.DTOs
{
    public class UpdateProfileDto
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(256)]
        public string UserName { get; set; } = null!;

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public string PasswordHash { get; set; } = null!;
    }
}
