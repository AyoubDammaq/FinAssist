using AuthService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Commands.Logout
{
    public class LogoutCommandHandler(IUserRepository userRepository, ILogger<LogoutCommandHandler> logger) : IRequestHandler<LogoutCommand, Unit>
    {
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly ILogger<LogoutCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Début du traitement de la commande de déconnexion pour l'utilisateur : {Email}", request.email);

            var user = await _userRepository.GetByEmail(request.email);
            if (user is null)
            {
                _logger.LogWarning("Aucun utilisateur trouvé avec l'email : {Email}", request.email);
                return Unit.Value;
            }

            user.RefreshTokenHash = null;
            user.RefreshTokenExpiryTime = null;

            await _userRepository.Update(user);

            _logger.LogInformation("Déconnexion réussie pour l'utilisateur : {Email}", request.email);

            return Unit.Value;
        }
    }
}
