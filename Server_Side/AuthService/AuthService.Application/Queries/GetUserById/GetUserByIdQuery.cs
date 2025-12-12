using AuthService.Application.DTOs;
using MediatR;

namespace AuthService.Application.Queries.GetUserById
{
    public record GetUserByIdQuery(Guid UserId) : IRequest<GetUserByIdDto>;
}
