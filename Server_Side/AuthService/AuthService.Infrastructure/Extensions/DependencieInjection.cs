using AuthService.Application.Utils;
using AuthService.Domain.Interfaces;
using AuthService.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AuthService.Infrastructure.Extensions
{
    public static class DependencieInjection
    {
        public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<Data.UserDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("AuthConnection")));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<PasswordManagement>();
            services.AddScoped<TokenManagement>();


            services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                var jwtKey = configuration.GetSection("JwtOptions:Key").Value;
                if (string.IsNullOrEmpty(jwtKey))
                {
                    throw new InvalidOperationException("La clé JWT (JwtOptions:Key) ne peut pas être nulle ou vide.");
                }

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration.GetSection("JwtOptions:Issuer").Value,
                    ValidAudience = configuration.GetSection("JwtOptions:Audience").Value,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });
            services.AddAuthorizationBuilder();

            // ✅ CORRECTION : Enregistrer MediatR depuis l'assembly Application
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(Application.Queries.GetAllUsers.GetAllUsersQuery).Assembly));

            // ✅ CORRECTION : Enregistrer AutoMapper depuis l'assembly Application
            services.AddAutoMapper(cfg => { }, typeof(Application.Mapping.UserMapper).Assembly);

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });
        }
    }
}
