using AuthService.Application.DTOs;
using AuthService.Application.Utils;
using AuthService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Commands.RefreshToken
{
    public sealed class RefreshTokenCommandHandler(
        IUserRepository userRepository,
        ITokenManagement tokenManagement,
        ILogger<RefreshTokenCommandHandler> logger)
        : IRequestHandler<RefreshTokenCommand, RefreshTokenResponseDto>
    {
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly ITokenManagement _tokenManagement = tokenManagement ?? throw new ArgumentNullException(nameof(tokenManagement));
        private readonly ILogger<RefreshTokenCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<RefreshTokenResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Début du traitement de la commande RefreshToken pour l'utilisateur Id: {UserId}", request.RefreshTokenRequest.Id);

            if (string.IsNullOrWhiteSpace(request.RefreshTokenRequest.RefreshToken))
            {
                _logger.LogWarning("Refresh token manquant pour l'utilisateur Id: {UserId}", request.RefreshTokenRequest.Id);
                throw new UnauthorizedAccessException("Refresh token manquant.");
            }

            var user = await _userRepository.GetById(request.RefreshTokenRequest.Id);

            if (user == null)
            {
                _logger.LogWarning("Utilisateur non trouvé pour l'Id: {UserId}", request.RefreshTokenRequest.Id);
                throw new UnauthorizedAccessException("Refresh token invalide.");
            }

            if (!user.RefreshTokenExpiryTime.HasValue || user.RefreshTokenExpiryTime.Value <= DateTime.UtcNow)
            {
                _logger.LogWarning("Refresh token expiré pour l'utilisateur Id: {UserId}", request.RefreshTokenRequest.Id);
                throw new UnauthorizedAccessException("Refresh token expiré.");
            }

            var newAccessToken = await _tokenManagement.GenerateToken(user);
            var newRefreshToken = await _tokenManagement.GenerateRefreshToken();

            user.RefreshTokenHash = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.Update(user);

            _logger.LogInformation("Nouveaux tokens générés et utilisateur mis à jour pour l'Id: {UserId}", user.Id);

            return new RefreshTokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }
    }
}
