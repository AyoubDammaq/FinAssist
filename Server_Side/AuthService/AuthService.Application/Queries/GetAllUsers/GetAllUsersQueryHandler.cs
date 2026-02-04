using AuthService.Application.DTOs;
using AuthService.Domain.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Queries.GetAllUsers
{
    public class GetAllUsersQueryHandler(
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<GetAllUsersQueryHandler> logger) : IRequestHandler<GetAllUsersQuery, List<GetAllUsersDto>>
    {
        private readonly IUserRepository _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        private readonly IMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        private readonly ILogger<GetAllUsersQueryHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public async Task<List<GetAllUsersDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Début de la récupération de tous les utilisateurs.");
            try
            {
                var users = await _userRepository.GetAll();
                var userDtos = _mapper.Map<List<GetAllUsersDto>>(users);
                _logger.LogInformation("Récupération de {Count} utilisateurs terminée avec succès.", userDtos.Count);
                return userDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des utilisateurs.");
                throw new ApplicationException("Une erreur est survenue lors de la récupération des utilisateurs.", ex);
            }
        }
    }
}
