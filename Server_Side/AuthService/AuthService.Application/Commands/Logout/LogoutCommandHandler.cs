using AuthService.Domain.Interfaces;
using MediatR;

namespace AuthService.Application.Commands.Logout
{
    public class LogoutCommandHandler(IUserRepository userRepository) : IRequestHandler<LogoutCommand, Unit>
    {
        private readonly IUserRepository _userRepository = userRepository;

        public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmail(request.email);
            if (user is null)
                return Unit.Value;

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            await _userRepository.Update(user);

            return Unit.Value;
        }
    }
}
