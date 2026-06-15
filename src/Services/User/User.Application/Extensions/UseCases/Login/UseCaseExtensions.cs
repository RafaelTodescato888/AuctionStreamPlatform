using Microsoft.Extensions.DependencyInjection;
using User.Application.UseCases.Login.Commands;
using User.Domain.Interfaces.UseCase.Login.Commands;

namespace User.Application.Extensions.UseCases.Login
{
    internal static class UseCaseExtensions
    {
        internal static IServiceCollection AddLoginUseCases(this IServiceCollection services)
        {
            return services.AddTransient<ILoginUseCase, LoginUseCase>();
        }
    }
}
