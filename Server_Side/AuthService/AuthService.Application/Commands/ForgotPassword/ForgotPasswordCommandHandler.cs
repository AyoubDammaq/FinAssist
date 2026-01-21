using AuthService.Application.Utils;
using AuthService.Domain.Interfaces;
using MediatR;

namespace AuthService.Application.Commands.ForgotPassword
{
    public sealed class ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        ITokenManagement tokenManagement)
        : IRequestHandler<ForgotPasswordCommand, Unit>
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly ITokenManagement _tokenManagement = tokenManagement;

        public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var email = request.ForgotPasswordRequestDto.Email;

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email requis.");
            }

            var user = await _userRepository.GetByEmail(email);

            if (user == null)
            {
                return Unit.Value;
            }

            var resetToken = await _tokenManagement.GenerateResetToken(); 
            user.ResetToken = resetToken;
            user.ResetTokenExpiryTime = DateTime.UtcNow.AddMinutes(15);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.Update(user);

            return Unit.Value;
        }
    }
}