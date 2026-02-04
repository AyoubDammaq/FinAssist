using AuthService.Domain.Entities;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Utils
{
    public class TokenManagement(IConfiguration configuration, ILogger<TokenManagement> logger) : ITokenManagement
    {
        public async Task<string> GenerateToken(User user)
        {
            try
            {
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim("userName", user.UserName),
                    new Claim("role", user.Role?.ToString() ?? "User")
                };

                var jwtKey = configuration["Jwt:Key"];
                if (string.IsNullOrEmpty(jwtKey))
                {
                    logger.LogError("La clé JWT n'est pas configurée.");
                    throw new InvalidOperationException("La clé JWT n'est pas configurée.");
                }

                var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var minutes = int.Parse(configuration["Jwt:ExpireMinutes"] ?? "60");

                var token = new JwtSecurityToken(
                    issuer: configuration["Jwt:Issuer"],
                    audience: configuration["Jwt:Audience"],
                    claims: claims,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddMinutes(minutes),
                    signingCredentials: creds
                );

                return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors de la génération du token.");
                throw;
            }
        }

        public async Task<string> GenerateRefreshToken()
        {
            try
            {
                var randomNumber = new byte[32];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomNumber);
                    return await Task.FromResult(Convert.ToBase64String(randomNumber));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors de la génération du refresh token.");
                throw;
            }
        }

        public async Task<string> GenerateResetToken()
        {
            try
            {
                var randomNumber = new byte[32];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomNumber);
                    var base64 = Convert.ToBase64String(randomNumber);
                    var base64Url = base64.Replace("+", "-").Replace("/", "_").TrimEnd('=');
                    return await Task.FromResult(base64Url);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors de la génération du reset token.");
                throw;
            }
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            try
            {
                var jwtKey = configuration["Jwt:Key"];
                if (string.IsNullOrEmpty(jwtKey))
                {
                    logger.LogError("La clé JWT n'est pas configurée.");
                    throw new InvalidOperationException("La clé JWT n'est pas configurée.");
                }

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey)),
                    ValidateLifetime = false,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"]
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    logger.LogError("Token JWT invalide.");
                    throw new SecurityTokenException("Invalid token");
                }

                return principal;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors de la récupération du principal depuis le token expiré.");
                throw;
            }
        }

        public bool ValidateToken(string token)
        {
            try
            {
                var jwtKey = configuration["Jwt:Key"];
                if (string.IsNullOrEmpty(jwtKey))
                {
                    logger.LogWarning("La clé JWT n'est pas configurée.");
                    return false;
                }

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey)),
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Token non valide.");
                return false;
            }
        }

        public string? GetUserIdFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                return jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors de la récupération de l'ID utilisateur depuis le token.");
                return null;
            }
        }

        public string HashToken(string token)
        {
            try
            {
                using var sha256 = SHA256.Create();
                var bytes = System.Text.Encoding.UTF8.GetBytes(token);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors du hash du token.");
                throw;
            }
        }

        public bool VerifyHashedToken(string token, string storedBase64Hash)
        {
            try
            {
                var computed = HashToken(token);
                // use constant-time compare to avoid timing attacks
                return CryptographicOperations.FixedTimeEquals(
                    Convert.FromBase64String(computed),
                    Convert.FromBase64String(storedBase64Hash)
                );
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erreur lors de la vérification du hash du token.");
                return false;
            }
        }
    }
}
