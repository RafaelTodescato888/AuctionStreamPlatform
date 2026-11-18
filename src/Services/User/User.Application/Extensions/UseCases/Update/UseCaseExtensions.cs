using Microsoft.Extensions.DependencyInjection;
using User.Application.UseCases.Update.Commands;
using User.Domain.Interfaces.UseCase.Update.Commands;

namespace User.Application.Extensions.UseCases.Update;

internal static class UseCaseExtensions
{
    internal static IServiceCollection AddUpdateUseCases(this IServiceCollection services)
    {
        services.AddTransient<IUpdateProfileUserUseCase, UpdateProfileUserUseCase>();

        return services;
    }
}
