using AuthService.Domain.Entities;
using System.Security.Claims;

namespace AuthService.Application.Utils;

public interface ITokenManagement
{
    Task<string> GenerateToken(User user);
    Task<string> GenerateRefreshToken();
    Task<string> GenerateResetToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    bool ValidateToken(string token);
    string? GetUserIdFromToken(string token);
}