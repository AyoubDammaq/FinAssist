using AuthService.Application.DTOs;
using MediatR;

namespace AuthService.Application.Queries.GetUserByEmail
{
    public record GetUserByEmailQuery(string Email) : IRequest<GetUserByEmailDto>;
}
