using CrossCutting.Kafka.Common.Consumers.Extensions;
using CrossCutting.Kafka.Common.Consumers.Options;
using CrossCutting.Kafka.Common.Producers.Extensions;
using CrossCutting.Kafka.Common.Producers.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using User.Domain.Interfaces.Repositories.Base;
using User.Domain.Interfaces.Repositories.User;
using User.Domain.Interfaces.Repositories.UserProfile;
using User.Domain.Interfaces.Repositories.UserSession;
using User.Domain.Interfaces.UoW;
using User.Infrastructure.Context;
using User.Infrastructure.Kafka.Consumers;
using User.Infrastructure.Kafka.Envelopes;
using User.Infrastructure.Repositories.Base;
using User.Infrastructure.Repositories.User;
using User.Infrastructure.Repositories.UserProfile;
using User.Infrastructure.Repositories.UserSession;
using User.Infrastructure.Seeds;
using User.Infrastructure.UoW;

namespace User.Infrastructure.Extensions;

public static class ConfigureInfrastructureExtensions
{
    public static IServiceCollection ConfigureInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .ConfigureContextDatabase(configuration)
            .ConfigureCaching(configuration)
            .ConfigureRepository()
            .ConfigureKafkaServices(configuration);
    }

    private static IServiceCollection ConfigureContextDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = Environment.GetEnvironmentVariable("CONTEXT_DATA_SOURCE")
                                   ?? configuration.GetConnectionString("CONTEXT_DATA_SOURCE")
                                   ?? throw new ArgumentNullException("Could not find the CONTEXT_DATA_SOURCE environment variable");

        services.AddDbContext<UserContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(connectionString);
            options.UseAsyncUsersProfileSeeding(serviceProvider);
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
        services.AddScoped<IUsersSessionRepository, UsersSessionRepository>();

        return services;
    }

    private static IServiceCollection ConfigureCaching(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = Environment.GetEnvironmentVariable("REDIS_DATA_SOURCE")
                                   ?? configuration.GetConnectionString("REDIS_DATA_SOURCE")
                                   ?? throw new ArgumentNullException("Could not find the REDIS_DATA_SOURCE environment variable");
            options.InstanceName = "AuctionStreamPlatform_";
        });

        return services;
    }

    private static IServiceCollection ConfigureKafkaServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddProducers(configuration);
        services.AddConsumers(configuration);

        return services;
    }

    private static IServiceCollection AddConsumers(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaConsumerOptions>(configuration.GetSection("Kafka:Consumer"));

        services.AddKafkaConsumer<UserCreatedConsumer, UserCreatedEnvelope>(configuration);

        return services;
    }

    private static IServiceCollection AddProducers(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaProducerOptions>(configuration.GetSection("Kafka:Producer"));

        services.AddKafkaProducer<UserCreatedEnvelope>(configuration);
        return services;
    }
}
