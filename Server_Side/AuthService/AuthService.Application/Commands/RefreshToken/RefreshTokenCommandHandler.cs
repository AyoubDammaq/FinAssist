using AuthService.Application.DTOs;
using AuthService.Application.Utils;
using AuthService.Domain.Interfaces;
using MediatR;

namespace AuthService.Application.Commands.RefreshToken
{
    public sealed class RefreshTokenCommandHandler(
        IUserRepository userRepository,
        TokenManagement tokenManagement)
        : IRequestHandler<RefreshTokenCommand, RefreshTokenResponseDto>
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly TokenManagement _tokenManagement = tokenManagement;

        public async Task<RefreshTokenResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshTokenRequest.RefreshToken))
            {
                throw new UnauthorizedAccessException("Refresh token manquant.");
            }

            var user = await _userRepository.GetById(request.RefreshTokenRequest.Id);

            if (user == null)
            {
                throw new UnauthorizedAccessException("Refresh token invalide.");
            }

            if (!user.RefreshTokenExpiryTime.HasValue || user.RefreshTokenExpiryTime.Value <= DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Refresh token expiré.");
            }

            var newAccessToken = await _tokenManagement.GenerateToken(user);
            var newRefreshToken = await _tokenManagement.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.Update(user);

            return new RefreshTokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }
    }
}
