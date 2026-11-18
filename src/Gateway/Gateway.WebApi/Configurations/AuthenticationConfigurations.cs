using CrossCutting.Extensions;

namespace Gateway.WebApi.Configurations;

internal static class AuthenticationConfigurations
{
    internal static IServiceCollection ConfigureRolesPolicyValidation(this IServiceCollection services, IConfiguration configurations)
    {
        services
            .AddAuthorizationBuilder()
            .ConfigureRoles();

        services.AddJwtCustomConfiguration(configurations);

        return services;
    }
}
