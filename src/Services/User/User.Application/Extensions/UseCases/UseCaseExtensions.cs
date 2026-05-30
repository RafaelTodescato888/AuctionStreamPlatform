using Microsoft.Extensions.DependencyInjection;

namespace User.Application.Extensions.UseCases
{
    public static class UseCaseExtensions
    {
        public static IServiceCollection AddUseCases(this IServiceCollection services)
        {
            return services;
        }
    }
}
