using AuthService.Application.Utils;
using AuthService.Domain.Interfaces;
using MediatR;

namespace AuthService.Application.Commands.ChangePassword
{
    public sealed class ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordManagement passwordManagement)
        : IRequestHandler<ChangePasswordCommand, Unit>
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IPasswordManagement _passwordManagement = passwordManagement;

        public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var dto = request.ChangePasswordRequestDto;

            if (string.IsNullOrWhiteSpace(dto.CurrentPassword) ||
                string.IsNullOrWhiteSpace(dto.NewPassword) ||
                string.IsNullOrWhiteSpace(dto.ConfirmNewPassword))
            {
                throw new ArgumentException("Champs requis manquants.");
            }

            if (!string.Equals(dto.NewPassword, dto.ConfirmNewPassword, StringComparison.Ordinal))
            {
                throw new ArgumentException("Les mots de passe ne correspondent pas.");
            }

            var user = await _userRepository.GetById(request.UserId);
            if (user == null)
            {
                throw new KeyNotFoundException("Utilisateur introuvable.");
            }

            var isPasswordValid = await _passwordManagement.VerifyPassword(dto.CurrentPassword, user.PasswordHash, user);
            if (!isPasswordValid)
            {
                throw new UnauthorizedAccessException("Mot de passe actuel incorrect.");
            }

            var hashResult = await _passwordManagement.HashPassword(dto.NewPassword);
            user.PasswordHash = hashResult.Hash;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.Update(user);

            return Unit.Value;
        }
    }
}