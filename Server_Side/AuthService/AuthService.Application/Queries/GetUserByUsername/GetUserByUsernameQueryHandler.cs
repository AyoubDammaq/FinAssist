using AuthService.Application.DTOs;
using AuthService.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AuthService.Application.Queries.GetUserByUsername
{
    public class GetUserByUsernameQueryHandler(IUserRepository userRepository, IMapper mapper) : IRequestHandler<GetUserByUsernameQuery, GetUserByUsernameDto>
	{
        private readonly IUserRepository _userRepository = userRepository;
		private readonly IMapper _mapper = mapper;

		public async Task<GetUserByUsernameDto> Handle(GetUserByUsernameQuery request, CancellationToken cancellationToken)
		{
			try
			{
				var user = await _userRepository.GetByUsername(request.Username);
				if (user == null)
				{
					throw new KeyNotFoundException($"Utilisateur avec le nom d'utilisateur {request.Username} non trouvé.");
				}
				var userDto = _mapper.Map<GetUserByUsernameDto>(user);
				return userDto;
			}
			catch (Exception ex)
			{
				throw new ApplicationException("Une erreur est survenue lors de la récupération de l'utilisateur par nom d'utilisateur.", ex);
			}
		}
	}
}
