using AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Utils
{
    public class PasswordManagement(ILogger<PasswordManagement> logger) : IPasswordManagement
    {
        private readonly ILogger<PasswordManagement> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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
                _logger.LogError(ex, "Erreur lors de la vérification du mot de passe pour l'utilisateur {UserId}.", userFromDb?.Id);
                throw new ApplicationException("Une erreur est survenue lors de la vérification du mot de passe.", ex);
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
                _logger.LogError(ex, "Erreur lors du hachage du mot de passe.");
                throw new ApplicationException("Une erreur est survenue lors du hachage du mot de passe.", ex);
            }
        }

        public Task<bool> IsPasswordStrong(string password)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification de la robustesse du mot de passe.");
                throw new ApplicationException("Une erreur est survenue lors de la vérification de la robustesse du mot de passe.", ex);
            }
        }
    }
}
