using AuthService.Application.DTOs;
using AuthService.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace AuthService.Application.Queries.GetAllUsers
{
    public class GetAllUsersQueryHandler(IUserRepository userRepository, IMapper mapper) : IRequestHandler<GetAllUsersQuery, List<GetAllUsersDto>>
	{
        public readonly IUserRepository _userRepository = userRepository;
		public readonly IMapper _mapper = mapper;

		public async Task<List<GetAllUsersDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
		{
			try
			{
				var users = await _userRepository.GetAll();
				var userDtos = _mapper.Map<List<GetAllUsersDto>>(users);
				return userDtos;
			}
			catch (Exception ex)
			{
				throw new ApplicationException("Une erreur est survenue lors de la récupération des utilisateurs.", ex);
			}
		}
	}
}
