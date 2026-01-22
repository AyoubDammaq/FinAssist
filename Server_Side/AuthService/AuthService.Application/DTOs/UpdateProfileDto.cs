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

        [StringLength(20)]
        [RegularExpression(@"^\s*$|^\+?[0-9\s\-\(\)\.]{7,20}$", ErrorMessage = "The PhoneNumber field is not a valid phone number.")]
        public string? PhoneNumber { get; set; }
    }
}
