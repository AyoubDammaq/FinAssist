using AuthService.Application.DTOs;
using AuthService.Domain.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Queries.GetUserByUsername
{
    public class GetUserByUsernameQueryHandler(
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<GetUserByUsernameQueryHandler> logger) : IRequestHandler<GetUserByUsernameQuery, GetUserByUsernameDto>
    {
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<GetUserByUsernameQueryHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<GetUserByUsernameDto> Handle(GetUserByUsernameQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Début de la récupération de l'utilisateur avec le nom d'utilisateur : {Username}", request.Username);
            try
            {
                var user = await _userRepository.GetByUsername(request.Username);
                if (user == null)
                {
                    _logger.LogWarning("Utilisateur avec le nom d'utilisateur {Username} non trouvé.", request.Username);
                    throw new KeyNotFoundException($"Utilisateur avec le nom d'utilisateur {request.Username} non trouvé.");
                }
                var userDto = _mapper.Map<GetUserByUsernameDto>(user);
                _logger.LogInformation("Utilisateur {Username} récupéré avec succès.", request.Username);
                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'utilisateur par nom d'utilisateur : {Username}", request.Username);
                throw new ApplicationException("Une erreur est survenue lors de la récupération de l'utilisateur par nom d'utilisateur.", ex);
            }
        }
    }
}
