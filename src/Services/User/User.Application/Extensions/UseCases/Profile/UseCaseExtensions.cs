using Microsoft.Extensions.DependencyInjection;
using User.Application.UseCases.Profile.Queries;
using User.Domain.Interfaces.UseCase.Profile.Queries;

namespace User.Application.Extensions.UseCases.Profile
{
    internal static class UseCaseExtensions
    {
        internal static IServiceCollection AddProfileUseCases(this IServiceCollection services)
        {
            services.AddTransient<IGetUserProfileUseCase, GetUserProfileUseCase>();

            return services;
        }
    }
}
