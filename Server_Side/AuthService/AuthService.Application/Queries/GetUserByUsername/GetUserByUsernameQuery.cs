using AuthService.Application.DTOs;
using MediatR;

namespace AuthService.Application.Queries.GetUserByUsername
{
    public record GetUserByUsernameQuery(string Username) : IRequest<GetUserByUsernameDto>;
}
