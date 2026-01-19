using AuthService.Application.DTOs;
using MediatR;

namespace AuthService.Application.Commands.ChangePassword
{
    public sealed record ChangePasswordCommand(Guid UserId, ChangePasswordRequestDto ChangePasswordRequestDto) : IRequest<Unit>;
}