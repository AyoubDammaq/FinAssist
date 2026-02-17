using Microsoft.Extensions.DependencyInjection;
using TransactionService.Application.Mappers;
using TransactionService.Domain.Interfaces;
using TransactionService.Infrastructure.Repositories;

namespace TransactionService.Infrastructure.Extensions
{
    public static class DependencieInjectionForApplication
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            // Register Mediator handlers
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencieInjectionForApplication).Assembly));

            // Register AutoMapper profiles
            services.AddAutoMapper(typeof(CategroyMappingProfile).Assembly);

            // Register repositories
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
        }
    }
}
