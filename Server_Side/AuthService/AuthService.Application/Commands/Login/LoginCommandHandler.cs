using AuthService.Application.DTOs;
using AuthService.Application.Utils;
using AuthService.Domain.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Commands.Login
{
    public class LoginCommandHandler(
            IUserRepository userRepository,
            IPasswordManagement passwordManagement,
            ITokenManagement tokenManagement,
            IMapper mapper,
            ILogger<LoginCommandHandler> logger) : IRequestHandler<LoginCommand, LoginResponseDto>
    {
        public readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        public readonly IPasswordManagement _passwordManagement = passwordManagement ?? throw new ArgumentNullException(nameof(passwordManagement));
        public readonly ITokenManagement _tokenManagement = tokenManagement ?? throw new ArgumentNullException(nameof(tokenManagement));
        public readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<LoginCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Tentative de connexion pour l'utilisateur : {Email}", request.loginRequestDto.Email);

                var user = await _userRepository.GetByEmail(request.loginRequestDto.Email);
                var isPasswordValid = user != null && await _passwordManagement.VerifyPassword(request.loginRequestDto.Password, user.PasswordHash, user);
                if (user == null || !isPasswordValid)
                {
                    _logger.LogWarning("Échec de connexion pour l'utilisateur : {Email} - Mot de passe ou email incorrect.", request.loginRequestDto.Email);
                    throw new UnauthorizedAccessException("Nom d'utilisateur ou mot de passe incorrect.");
                }

                var accessToken = await _tokenManagement.GenerateToken(user);
                var refreshToken = await _tokenManagement.GenerateRefreshToken();

                // Persistance du refresh token en base (GetByEmail() est AsNoTracking())
                var userTracked = await _userRepository.GetById(user.Id);
                if (userTracked is null)
                {
                    _logger.LogError("Utilisateur introuvable après authentification : {Email}", request.loginRequestDto.Email);
                    throw new KeyNotFoundException("Utilisateur introuvable.");
                }

                userTracked.RefreshTokenHash = refreshToken;
                userTracked.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // à ajuster selon ta politique
                await _userRepository.Update(userTracked);

                _logger.LogInformation("Connexion réussie pour l'utilisateur : {Email}", request.loginRequestDto.Email);

                return new LoginResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };
            }
            catch (UnauthorizedAccessException)
            {
                _logger.LogWarning("Tentative de connexion non autorisée pour l'utilisateur : {Email}", request.loginRequestDto.Email);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la tentative de connexion pour l'utilisateur : {Email}", request.loginRequestDto.Email);
                throw new ApplicationException("Une erreur est survenue lors de la tentative de connexion.", ex);
            }
        }
    }
}
