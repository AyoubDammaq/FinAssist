using AuthService.Application.DTOs;
using MediatR;

namespace AuthService.Application.Commands.UpdateProfile
{
    public record UpdateProfileCommand(UpdateProfileDto ProfileDto) : IRequest<Unit>;
}
