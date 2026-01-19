using MediatR;

namespace AuthService.Application.Queries.Health
{
    public class HealthCommandHandler : IRequestHandler<HealthCommand, string>
    {
        public Task<string> Handle(HealthCommand request, CancellationToken cancellationToken)
        {
            return Task.FromResult("AuthService is healthy");
        }
    }
}
