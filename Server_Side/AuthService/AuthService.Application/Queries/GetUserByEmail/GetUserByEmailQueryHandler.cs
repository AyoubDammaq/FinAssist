using AuthService.Application.DTOs;
using AuthService.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AuthService.Application.Queries.GetUserByEmail
{
    public class GetUserByEmailQueryHandler(IUserRepository userRepository, IMapper mapper) : IRequestHandler<GetUserByEmailQuery, GetUserByEmailDto>
	{
        public readonly IUserRepository _userRepository = userRepository;
		public readonly IMapper _mapper = mapper;

		public async Task<GetUserByEmailDto> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
		{
			try
			{
				var user = await _userRepository.GetByEmail(request.Email);
				if (user == null)
				{
					throw new KeyNotFoundException($"Utilisateur avec l'email {request.Email} non trouvé.");
				}
				var userDto = _mapper.Map<GetUserByEmailDto>(user);
				return userDto;
			}
			catch (Exception ex)
			{
				throw new ApplicationException("Une erreur est survenue lors de la récupération de l'utilisateur par email.", ex);
			}
		}
	}
}
