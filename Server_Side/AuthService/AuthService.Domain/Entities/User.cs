using AuthService.Domain.Common;
using AuthService.Domain.Enums;

namespace AuthService.Domain.Entities;

public partial class User : BaseEntity
{
    public string UserName { get; set; } = null!;
	public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public UserRole? Role { get; set; } = UserRole.User;

    public bool IsEmailConfirmed { get; set; } = false;
    public string? PhoneNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsLocked { get; set; }
    public DateTime? LockoutEnd { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiryTime { get; set; }
}
