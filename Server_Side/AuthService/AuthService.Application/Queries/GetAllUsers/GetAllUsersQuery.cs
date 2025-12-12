using AuthService.Application.DTOs;
using MediatR;

namespace AuthService.Application.Queries.GetAllUsers
{
    public record GetAllUsersQuery : IRequest<List<GetAllUsersDto>>;
}
