using AuthService.Application.Utils;
using AuthService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Commands.ResetPassword
{
    public sealed class ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordManagement passwordManagement,
        ITokenManagement tokenManagement,
        IEmailManagment emailManagment,
        ILogger<ResetPasswordCommandHandler> logger)
        : IRequestHandler<ResetPasswordCommand, Unit>
    {
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly IPasswordManagement _passwordManagement = passwordManagement ?? throw new ArgumentNullException(nameof(passwordManagement));
        private readonly ITokenManagement _tokenManagement = tokenManagement ?? throw new ArgumentNullException(nameof(tokenManagement));
        private readonly IEmailManagment _emailManagment = emailManagment ?? throw new ArgumentNullException(nameof(emailManagment));
        private readonly ILogger<ResetPasswordCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var dto = request.ResetPasswordRequestDto;

            _logger.LogInformation("Début du traitement de la réinitialisation du mot de passe pour l'email : {Email}", dto.Email);

            if (string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.ResetToken) ||
                string.IsNullOrWhiteSpace(dto.NewPassword) ||
                string.IsNullOrWhiteSpace(dto.ConfirmNewPassword))
            {
                _logger.LogWarning("Champs requis manquants pour la réinitialisation du mot de passe. Email: {Email}", dto.Email);
                throw new ArgumentException("Champs requis manquants.");
            }

            if (!string.Equals(dto.NewPassword, dto.ConfirmNewPassword, StringComparison.Ordinal))
            {
                _logger.LogWarning("Les mots de passe ne correspondent pas pour l'email : {Email}", dto.Email);
                throw new ArgumentException("Les mots de passe ne correspondent pas.");
            }

            var isStrong = await _passwordManagement.IsPasswordStrong(dto.NewPassword);
            if (!isStrong)
            {
                _logger.LogWarning("Mot de passe trop faible pour l'email : {Email}", dto.Email);
                throw new ArgumentException("Le mot de passe n'est pas assez fort.");
            }

            var user = await _userRepository.GetByEmail(dto.Email);

            if (user == null)
            {
                _logger.LogWarning("Aucun utilisateur trouvé pour l'email : {Email}", dto.Email);
                return Unit.Value;
            }

            if (string.IsNullOrWhiteSpace(user.ResetTokenHash) ||
                !string.Equals(user.ResetTokenHash, _tokenManagement.HashToken(dto.ResetToken), StringComparison.Ordinal) ||
                !user.ResetTokenExpiryTime.HasValue ||
                user.ResetTokenExpiryTime.Value <= DateTime.UtcNow)
            {
                _logger.LogWarning("Reset token invalide ou expiré pour l'email : {Email}", dto.Email);
                throw new UnauthorizedAccessException("Reset token invalide ou expiré.");
            }

            var hashResult = await _passwordManagement.HashPassword(dto.NewPassword).ConfigureAwait(false);
            user.PasswordHash = hashResult.Hash;

            user.ResetTokenHash = null;
            user.ResetTokenExpiryTime = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.Update(user).ConfigureAwait(false);

            _logger.LogInformation("Mot de passe réinitialisé avec succès pour l'email : {Email}", user.Email);

            // Optionnel : envoyer un email de notification "mot de passe modifié" via IEmailManagment
            await _emailManagment.SendPasswordChangedEmail(user.Email).ConfigureAwait(false);

            _logger.LogInformation("Email de notification de changement de mot de passe envoyé à : {Email}", user.Email);

            return Unit.Value;
        }
    }
}