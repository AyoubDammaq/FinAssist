using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Infrastructure.Extensions
{
    public static class DependencieInjection
    {
        public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<Data.UserDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("UserDbConnection")));

            services.AddScoped<Domain.Interfaces.IUserRepository, Repositories.UserRepository>();
        }
    }
}
