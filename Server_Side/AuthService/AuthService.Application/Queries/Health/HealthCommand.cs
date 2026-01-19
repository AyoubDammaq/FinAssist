using MediatR;

namespace AuthService.Application.Queries.Health
{
    public record HealthCommand() : IRequest<string>;
}
