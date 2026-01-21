using AuthService.Application.Utils;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Infrastructure.Extensions
{
    public static class DependencieInjectionForApplication
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Enregistrement de MediatR avec tous les handlers de l'assembly Application
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(Application.Queries.GetAllUsers.GetAllUsersQuery).Assembly));

            // Enregistrement d'AutoMapper avec tous les profils de l'assembly Application
            services.AddAutoMapper(cfg => { }, typeof(Application.Mapping.UserMapper).Assembly);

            // ✅ FluentValidation : enregistre tous les validators de l'assembly Application
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssembly(typeof(Application.Commands.Login.LoginCommandValidator).Assembly);

            // Enregistrement des services Utils
            services.AddScoped<PasswordManagement>();
            services.AddScoped<TokenManagement>();

            return services;
        }
    }
}
