using AuthService.Domain.Entities;

namespace AuthService.Application.Utils;

public interface IPasswordManagement
{
    Task<bool> VerifyPassword(string enteredPassword, string storedHashedPassword, User userFromDb);
    Task<(string Hash, string Salt)> HashPassword(string password);
    Task<bool> IsPasswordStrong(string password);
}
