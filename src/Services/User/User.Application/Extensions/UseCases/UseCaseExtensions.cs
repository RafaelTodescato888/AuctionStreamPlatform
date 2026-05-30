using Microsoft.Extensions.DependencyInjection;
using User.Application.Extensions.UseCases.Login;
using User.Application.Extensions.UseCases.Profile;
using User.Application.Extensions.UseCases.Register;
using User.Application.Extensions.UseCases.Update;

namespace User.Application.Extensions.UseCases;

public static class UseCaseExtensions
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        return services.AddRegisterUseCases()
            .AddLoginUseCases()
            .AddProfileUseCases()
            .AddUpdateUseCases();
    }
}
