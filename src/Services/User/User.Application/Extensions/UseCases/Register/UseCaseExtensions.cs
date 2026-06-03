using Microsoft.Extensions.DependencyInjection;
using User.Application.UseCases.Register.Commands;
using User.Domain.Interfaces.UseCase.Register.Commands;

namespace User.Application.Extensions.UseCases.Register
{
    internal static class UseCaseExtensions
    {
        internal static IServiceCollection AddRegisterUseCases(this IServiceCollection services)
        {
            services.AddTransient<IRegisterUserUseCase, RegisterUserUseCase>();

            return services;
        }
    }
}
