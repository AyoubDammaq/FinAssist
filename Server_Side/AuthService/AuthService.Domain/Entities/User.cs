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

}
