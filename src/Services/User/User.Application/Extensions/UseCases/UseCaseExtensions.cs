using Microsoft.Extensions.DependencyInjection;
using User.Application.Extensions.UseCases.Register;

namespace User.Application.Extensions.UseCases
{
    public static class UseCaseExtensions
    {
        public static IServiceCollection AddUseCases(this IServiceCollection services)
        {
            return services.AddRegisterUseCases();
        }
    }
}
