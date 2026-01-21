using MediatR;

namespace AuthService.Application.Commands.Logout
{
    public record LogoutCommand(string email) : IRequest<Unit>;
}
