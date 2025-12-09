using AuthService.Domain.Common;
using AuthService.Domain.Enums;

namespace AuthService.Domain.Models;

public partial class User : BaseEntity
{

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public UserRole? Role { get; set; } 

    public bool IsEmailConfirmed { get; set; }
}
