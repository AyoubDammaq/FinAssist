using AuthService.Application.DTOs;
using MediatR;

namespace AuthService.Application.Commands.ResetPassword
{
    public sealed record ResetPasswordCommand(ResetPasswordRequestDto ResetPasswordRequestDto) : IRequest<Unit>;
}