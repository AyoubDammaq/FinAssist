using AuthService.Application.Utils;
using AuthService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Commands.ChangePassword
{
    public sealed class ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordManagement passwordManagement,
        ILogger<ChangePasswordCommandHandler> logger)
        : IRequestHandler<ChangePasswordCommand, Unit>
    {
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly IPasswordManagement _passwordManagement = passwordManagement ?? throw new ArgumentNullException(nameof(passwordManagement));
        private readonly ILogger<ChangePasswordCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var dto = request.ChangePasswordRequestDto;

            _logger.LogInformation("Début du changement de mot de passe pour l'utilisateur {UserId}", dto.UserId);

            if (string.IsNullOrWhiteSpace(dto.CurrentPassword) ||
                string.IsNullOrWhiteSpace(dto.NewPassword) ||
                string.IsNullOrWhiteSpace(dto.ConfirmNewPassword))
            {
                _logger.LogWarning("Champs requis manquants pour l'utilisateur {UserId}", dto.UserId);
                throw new ArgumentException("Champs requis manquants.");
            }

            if (!string.Equals(dto.NewPassword, dto.ConfirmNewPassword, StringComparison.Ordinal))
            {
                _logger.LogWarning("Les mots de passe ne correspondent pas pour l'utilisateur {UserId}", dto.UserId);
                throw new ArgumentException("Les mots de passe ne correspondent pas.");
            }

            var isStrong = await _passwordManagement.IsPasswordStrong(dto.NewPassword);
            if (!isStrong)
            {
                _logger.LogWarning("Le mot de passe n'est pas assez fort pour l'utilisateur {UserId}", dto.UserId);
                throw new ArgumentException("Le mot de passe n'est pas assez fort.");
            }

            var user = await _userRepository.GetById(request.ChangePasswordRequestDto.UserId);
            if (user == null)
            {
                _logger.LogWarning("Utilisateur introuvable : {UserId}", dto.UserId);
                throw new KeyNotFoundException("Utilisateur introuvable.");
            }

            var isPasswordValid = await _passwordManagement.VerifyPassword(dto.CurrentPassword, user.PasswordHash, user);
            if (!isPasswordValid)
            {
                _logger.LogWarning("Mot de passe actuel incorrect pour l'utilisateur {UserId}", dto.UserId);
                throw new UnauthorizedAccessException("Mot de passe actuel incorrect.");
            }

            var hashResult = await _passwordManagement.HashPassword(dto.NewPassword);
            user.PasswordHash = hashResult.Hash;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.Update(user);

            _logger.LogInformation("Mot de passe changé avec succès pour l'utilisateur {UserId}", dto.UserId);

            return Unit.Value;
        }
    }
}