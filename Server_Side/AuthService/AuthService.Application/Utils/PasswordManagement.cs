using System.Security.Cryptography;

namespace AuthService.Application.Utils
{
    public class PasswordManagement
    {
        public Task<bool> VerifyPassword(string enteredPassword, string storedHash, string storedSalt)
        {
            try
            {
                using (var hmac = new HMACSHA512(Convert.FromBase64String(storedSalt)))
                {
                    var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(enteredPassword));
                    var storedHashBytes = Convert.FromBase64String(storedHash);
                    return Task.FromResult(computedHash.SequenceEqual(storedHashBytes));
                }
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
                using (var hmac = new HMACSHA512())
                {
                    var salt = hmac.Key;
                    var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                    return Task.FromResult((Convert.ToBase64String(hash), Convert.ToBase64String(salt)));
                }
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
