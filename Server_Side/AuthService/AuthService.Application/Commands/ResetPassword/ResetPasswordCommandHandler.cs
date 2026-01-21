using AuthService.Application.Utils;
using AuthService.Domain.Interfaces;
using MediatR;

namespace AuthService.Application.Commands.ResetPassword
{
    public sealed class ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordManagement passwordManagement)
        : IRequestHandler<ResetPasswordCommand, Unit>
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IPasswordManagement _passwordManagement = passwordManagement;

        public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var dto = request.ResetPasswordRequestDto;

            if (string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.ResetToken) ||
                string.IsNullOrWhiteSpace(dto.NewPassword) ||
                string.IsNullOrWhiteSpace(dto.ConfirmNewPassword))
            {
                throw new ArgumentException("Champs requis manquants.");
            }

            if (!string.Equals(dto.NewPassword, dto.ConfirmNewPassword, StringComparison.Ordinal))
            {
                throw new ArgumentException("Les mots de passe ne correspondent pas.");
            }

            var user = await _userRepository.GetByEmail(dto.Email);

            if (user == null)
            {
                return Unit.Value;
            }

            if (string.IsNullOrWhiteSpace(user.ResetToken) ||
                !string.Equals(user.ResetToken, dto.ResetToken, StringComparison.Ordinal) ||
                !user.ResetTokenExpiryTime.HasValue ||
                user.ResetTokenExpiryTime.Value <= DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Reset token invalide ou expiré.");
            }

            var hashResult = await _passwordManagement.HashPassword(dto.NewPassword);
            user.PasswordHash = hashResult.Hash;

            // Invalider le token après usage
            user.ResetToken = null;
            user.ResetTokenExpiryTime = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.Update(user);

            return Unit.Value;
        }
    }
}