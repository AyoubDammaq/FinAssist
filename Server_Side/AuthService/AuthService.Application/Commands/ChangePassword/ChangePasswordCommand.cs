using AuthService.Application.DTOs;
using MediatR;

namespace AuthService.Application.Commands.ChangePassword
{
    public sealed record ChangePasswordCommand(ChangePasswordRequestDto ChangePasswordRequestDto) : IRequest<Unit>;
}