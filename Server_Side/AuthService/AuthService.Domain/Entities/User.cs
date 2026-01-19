using System.ComponentModel.DataAnnotations;
using AuthService.Domain.Common;
using AuthService.Domain.Enums;

namespace AuthService.Domain.Entities;

public partial class User : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string UserName { get; set; } = null!;

    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(512)]
    public string PasswordHash { get; set; } = null!;

    public UserRole? Role { get; set; } = UserRole.User;

    public bool IsEmailConfirmed { get; set; } = false;

    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsLocked { get; set; }

    public DateTime? LockoutEnd { get; set; }

    [StringLength(500)]
    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }

    [StringLength(500)]
    public string? ResetToken { get; set; }

    public DateTime? ResetTokenExpiryTime { get; set; }
}
