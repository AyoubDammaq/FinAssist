using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Application.Utils
{
    public class PasswordManagement : IPasswordManagement
    {
        public Task<bool> VerifyPassword(string enteredPassword, string storedHashedPassword, User userFromDb)
        {
            try
            {
                var passwordHasher = new PasswordHasher<User>();
                var result = passwordHasher.VerifyHashedPassword(userFromDb, storedHashedPassword, enteredPassword);
                return Task.FromResult(result == PasswordVerificationResult.Success
                                       || result == PasswordVerificationResult.SuccessRehashNeeded);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while verifying the password.", ex);
            }
        }

        public Task<(string Hash, string Salt)> HashPassword(string password)
        {
            try
            {
                var passwordHasher = new PasswordHasher<User>();
                var hash = passwordHasher.HashPassword(user: null!, password);

                return Task.FromResult((hash, string.Empty));
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while hashing the password.", ex);
            }
        }

        public Task<bool> IsPasswordStrong(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return Task.FromResult(false);
            bool hasUpper = false, hasLower = false, hasDigit = false, hasSpecial = false;
            foreach (var c in password)
            {
                if (char.IsUpper(c)) hasUpper = true;
                else if (char.IsLower(c)) hasLower = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else if (!char.IsLetterOrDigit(c)) hasSpecial = true;
            }
            return Task.FromResult(hasUpper && hasLower && hasDigit && hasSpecial);
        }
    }
}
