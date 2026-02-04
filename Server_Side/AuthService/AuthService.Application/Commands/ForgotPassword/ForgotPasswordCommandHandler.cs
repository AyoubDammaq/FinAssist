using AuthService.Application.Utils;
using AuthService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Commands.ForgotPassword
{
    public sealed class ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        ITokenManagement tokenManagement,
        IEmailManagment emailManagment,
        ILogger<ForgotPasswordCommandHandler> logger) : IRequestHandler<ForgotPasswordCommand, Unit>
    {
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly ITokenManagement _tokenManagement = tokenManagement ?? throw new ArgumentNullException(nameof(tokenManagement));
        private readonly IEmailManagment _emailManagment = emailManagment ?? throw new ArgumentNullException(nameof(emailManagment));
        private readonly ILogger<ForgotPasswordCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var email = request.ForgotPasswordRequestDto.Email?.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Tentative de réinitialisation de mot de passe sans email fourni.");
                throw new ArgumentException("Email requis.");
            }

            bool canSend;
            try
            {
                canSend = await _emailManagment.CheckEmailValidation(email, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la validation de l'email {Email}.", email);
                canSend = false;
            }

            if (!canSend)
            {
                _logger.LogInformation("Validation de l'email échouée ou email non autorisé pour {Email}.", email);
                return Unit.Value;
            }

            var normalized = email.ToLowerInvariant();
            var user = await _userRepository.GetByEmail(normalized).ConfigureAwait(false);
            if (user == null)
            {
                _logger.LogInformation("Aucun utilisateur trouvé pour l'email {Email}.", normalized);
                return Unit.Value;
            }

            var resetToken = await _tokenManagement.GenerateResetToken().ConfigureAwait(false);
            var resetTokenHash = _tokenManagement.HashToken(resetToken);

            user.ResetTokenHash = resetTokenHash;
            user.ResetTokenExpiryTime = DateTime.UtcNow.AddMinutes(15);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.Update(user).ConfigureAwait(false);
            _logger.LogInformation("Token de réinitialisation généré et enregistré pour l'utilisateur {UserId}.", user.Id);

            var emailSent = await _emailManagment.SendPasswordResetEmailAsync(email, resetToken, cancellationToken).ConfigureAwait(false);
            if (!emailSent)
            {
                _logger.LogError("Échec lors de l'envoi de l'e-mail de réinitialisation à {Email}.", email);
                throw new InvalidOperationException("Échec lors de l'envoi de l'e-mail de réinitialisation.");
            }

            _logger.LogInformation("E-mail de réinitialisation envoyé avec succès à {Email}.", email);
            return Unit.Value;
        }
    }
}