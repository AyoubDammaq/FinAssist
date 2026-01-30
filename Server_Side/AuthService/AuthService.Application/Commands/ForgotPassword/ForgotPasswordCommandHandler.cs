using AuthService.Application.Utils;
using AuthService.Domain.Interfaces;
using MediatR;

namespace AuthService.Application.Commands.ForgotPassword
{
    public sealed class ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        ITokenManagement tokenManagement,
        IEmailManagment emailManagment) : IRequestHandler<ForgotPasswordCommand, Unit>
    {
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly ITokenManagement _tokenManagement = tokenManagement ?? throw new ArgumentNullException(nameof(tokenManagement));
        private readonly IEmailManagment _emailManagment = emailManagment ?? throw new ArgumentNullException(nameof(emailManagment));

        public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var email = request.ForgotPasswordRequestDto.Email?.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email requis.");
            }

            bool canSend;
            try
            {
                canSend = await _emailManagment.CheckEmailValidation(email, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                canSend = false;
            }

            if (!canSend)
            {
                return Unit.Value;
            }

            // Récupérer l'utilisateur (possible double lecture, acceptable ; optimisation possible)
            var normalized = email.ToLowerInvariant();
            var user = await _userRepository.GetByEmail(normalized).ConfigureAwait(false);
            if (user == null)
            {
                // Course condition improbable : ne pas divulguer
                return Unit.Value;
            }

            var resetToken = await _tokenManagement.GenerateResetToken().ConfigureAwait(false);
            user.ResetToken = resetToken;
            user.ResetTokenExpiryTime = DateTime.UtcNow.AddMinutes(15);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.Update(user).ConfigureAwait(false);

            // Tenter d'envoyer l'email (ne pas propager l'exception vers l'appelant)
            try
            {
                await _emailManagment.SendPasswordResetEmailAsync(email, resetToken, cancellationToken).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Failed to send password reset email to {email}: {ex.Message}");
                Console.WriteLine(ex.ToString());
            }

            return Unit.Value;
        }
    }
}