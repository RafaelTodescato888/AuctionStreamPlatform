using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using User.Domain.Interfaces.Repositories.Base;
using User.Domain.Interfaces.Repositories.User;
using User.Domain.Interfaces.Repositories.UserProfile;
using User.Domain.Interfaces.UoW;
using User.Infrastructure.Context;
using User.Infrastructure.Repositories.Base;
using User.Infrastructure.Repositories.User;
using User.Infrastructure.Repositories.UserProfile;
using User.Infrastructure.UoW;

namespace User.Infrastructure.Extensions
{
    public static class ConfigureInfrastructureExtensions
    {
        public static IServiceCollection ConfigureInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .ConfigureContextDatabase(configuration)
                .ConfigureCaching(configuration)
                .ConfigureRepository();
        }

        private static IServiceCollection ConfigureContextDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = Environment.GetEnvironmentVariable("CONTEXT_DATA_SOURCE")
                                       ?? configuration.GetConnectionString("CONTEXT_DATA_SOURCE")
                                       ?? throw new ArgumentNullException("Não foi possível encontrar a string de conexão para CONTEXT_DATA_SOURCE");

            services.AddDbContext<AuctionStreamPlatformContext>(options =>
            {
                options.UseNpgsql(connectionString);
                options.LogTo(Console.WriteLine, LogLevel.Information);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });

            return services;
        }

        private static IServiceCollection ConfigureRepository(this IServiceCollection services)
        {
            services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IUsersProfileRepository, UsersProfileRepository>();
            services.AddScoped<IUsersRepository, UsersRepository>();

            return services;
        }

        private static IServiceCollection ConfigureCaching(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMemoryCache();

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = Environment.GetEnvironmentVariable("REDIS_DATA_SOURCE")
                                       ?? configuration.GetConnectionString("REDIS_DATA_SOURCE")
                                       ?? throw new ArgumentNullException("Não foi possível encontrar a string de conexão para REDIS_DATA_SOURCE");
                options.InstanceName = "AuctionStreamPlatform_";
            });

            return services;
        }
    }
}
