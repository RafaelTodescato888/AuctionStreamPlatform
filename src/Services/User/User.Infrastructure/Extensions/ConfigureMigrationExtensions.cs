using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using User.Infrastructure.Context;

namespace User.Infrastructure.Extensions
{
    public static class ConfigureMigrationExtensions
    {
        public static void ConfigureMigrations(this IServiceProvider serviceProvider)
        {
            using var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<AuctionStreamPlatformContext>();

            context?.Database.Migrate();
        }
    }
}
