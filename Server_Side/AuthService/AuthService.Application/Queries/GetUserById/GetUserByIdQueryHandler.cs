using AuthService.Application.DTOs;
using AuthService.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AuthService.Application.Queries.GetUserById
{
    public class GetUserByIdQueryHandler(IUserRepository userRepository, IMapper mapper) : IRequestHandler<GetUserByIdQuery, GetUserByIdDto>
	{
        public readonly IUserRepository _userRepository = userRepository;
		public readonly IMapper _mapper = mapper;

		public async Task<GetUserByIdDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
		{
			try
			{
				var user = await _userRepository.GetById(request.UserId);
				if (user == null)
				{
					throw new KeyNotFoundException($"Utilisateur avec l'ID {request.UserId} non trouvé.");
				}
				var userDto = _mapper.Map<GetUserByIdDto>(user);
				return userDto;
			}
			catch (Exception ex)
			{
				throw new ApplicationException("Une erreur est survenue lors de la récupération de l'utilisateur par ID.", ex);
			}
		}
	}
}
