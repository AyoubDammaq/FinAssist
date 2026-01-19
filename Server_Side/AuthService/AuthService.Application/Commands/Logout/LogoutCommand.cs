using AuthService.Domain.Entities;
using MediatR;

namespace AuthService.Application.Commands.Logout
{
    public record LogoutCommand(User user) : IRequest<Unit>;
}
