using AuthService.Application.DTOs;
using MediatR;

namespace AuthService.Application.Commands.ForgotPassword
{
    public sealed record ForgotPasswordCommand(ForgotPasswordRequestDto ForgotPasswordRequestDto) : IRequest<Unit>;
}