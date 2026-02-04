using AuthService.Application.DTOs;
using AuthService.Domain.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Queries.GetUserById
{
    public class GetUserByIdQueryHandler(IUserRepository userRepository, IMapper mapper, ILogger<GetUserByIdQueryHandler> logger) : IRequestHandler<GetUserByIdQuery, GetUserByIdDto>
    {
        public readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        public readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<GetUserByIdQueryHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<GetUserByIdDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Début de la récupération de l'utilisateur avec l'ID {UserId}", request.UserId);
            try
            {
                var user = await _userRepository.GetById(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning("Utilisateur avec l'ID {UserId} non trouvé.", request.UserId);
                    throw new KeyNotFoundException($"Utilisateur avec l'ID {request.UserId} non trouvé.");
                }
                var userDto = _mapper.Map<GetUserByIdDto>(user);
                _logger.LogInformation("Utilisateur avec l'ID {UserId} récupéré avec succès.", request.UserId);
                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'utilisateur avec l'ID {UserId}", request.UserId);
                throw new ApplicationException("Une erreur est survenue lors de la récupération de l'utilisateur par ID.", ex);
            }
        }
    }
}
