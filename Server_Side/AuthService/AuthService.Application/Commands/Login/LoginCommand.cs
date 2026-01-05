using AuthService.Application.DTOs;
using MediatR;

namespace AuthService.Application.Commands.Login
{
    public record LoginCommand(LoginRequestDto loginRequestDto) : IRequest<LoginResponseDto>;
}
