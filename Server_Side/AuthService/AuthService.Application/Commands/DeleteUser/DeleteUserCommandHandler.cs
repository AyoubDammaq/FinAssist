using AuthService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Commands.DeleteUser
{
    public class DeleteUserCommandHandler(IUserRepository userRepository, ILogger<DeleteUserCommandHandler> logger) : IRequestHandler<DeleteUserCommand, Unit>
    {
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly ILogger<DeleteUserCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Tentative de suppression de l'utilisateur avec l'ID {UserId}.", request.Id);
                var user = await _userRepository.GetById(request.Id);
                if (user == null)
                {
                    _logger.LogWarning("Utilisateur avec l'ID {UserId} non trouvé.", request.Id);
                    throw new KeyNotFoundException($"Utilisateur avec l'ID {request.Id} non trouvé.");
                }
                await _userRepository.Delete(user);
                _logger.LogInformation("Utilisateur avec l'ID {UserId} supprimé avec succès.", request.Id);
                return Unit.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de l'utilisateur avec l'ID {UserId}.", request.Id);
                throw new ApplicationException("Une erreur est survenue lors de la suppression de l'utilisateur.", ex);
            }
        }
    }
}
