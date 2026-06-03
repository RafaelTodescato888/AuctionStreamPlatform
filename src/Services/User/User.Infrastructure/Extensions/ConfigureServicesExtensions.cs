using Microsoft.Extensions.DependencyInjection;
using User.Domain.Interfaces.Services.Authentication.Register;
using User.Infrastructure.Services.Authentication.Register;

namespace User.Infrastructure.Extensions
{
    public static class ConfigureServicesExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IPasswordHashService, PasswordHashService>();

            return services;
        }
    }
}
