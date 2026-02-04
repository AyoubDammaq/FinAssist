using AuthService.Application.DTOs;
using AuthService.Domain.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Queries.GetUserByEmail
{
    public class GetUserByEmailQueryHandler(IUserRepository userRepository, IMapper mapper, ILogger<GetUserByEmailQueryHandler> logger) : IRequestHandler<GetUserByEmailQuery, GetUserByEmailDto>
    {
        public readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        public readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<GetUserByEmailQueryHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<GetUserByEmailDto> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Début de la récupération de l'utilisateur avec l'email : {Email}", request.Email);
            try
            {
                var user = await _userRepository.GetByEmail(request.Email);
                if (user == null)
                {
                    _logger.LogWarning("Utilisateur avec l'email {Email} non trouvé.", request.Email);
                    throw new KeyNotFoundException($"Utilisateur avec l'email {request.Email} non trouvé.");
                }
                var userDto = _mapper.Map<GetUserByEmailDto>(user);
                _logger.LogInformation("Utilisateur avec l'email {Email} récupéré avec succès.", request.Email);
                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'utilisateur avec l'email {Email}.", request.Email);
                throw new ApplicationException("Une erreur est survenue lors de la récupération de l'utilisateur par email.", ex);
            }
        }
    }
}
