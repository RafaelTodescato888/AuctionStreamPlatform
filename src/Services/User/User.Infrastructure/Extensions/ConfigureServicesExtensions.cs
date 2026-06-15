using Microsoft.Extensions.DependencyInjection;
using User.Domain.Interfaces.Services.Authentication.Login;
using User.Domain.Interfaces.Services.Authentication.Register;
using User.Domain.Interfaces.Services.Caching;
using User.Infrastructure.Services.Authentication.Login;
using User.Infrastructure.Services.Authentication.Register;
using User.Infrastructure.Services.Caching;

namespace User.Infrastructure.Extensions
{
    public static class ConfigureServicesExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IPasswordHashService, PasswordHashService>();

            services.AddScoped<IGenerateTokenService, GenerateTokenService>();

            services.AddScoped<ICachingService, CachingService>();

            return services;
        }
    }
}
