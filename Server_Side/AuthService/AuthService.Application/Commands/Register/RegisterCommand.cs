using AuthService.Application.DTOs;
using MediatR;

namespace AuthService.Application.Commands.Register
{
    public record RegisterCommand(RegisterRequestDto registerRequestDto) : IRequest<RegisterResponseDto>;
}
