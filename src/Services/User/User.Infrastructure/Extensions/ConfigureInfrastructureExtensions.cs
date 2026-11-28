using CrossCutting.Kafka.Common.Consumers.Extensions;
using CrossCutting.Kafka.Common.Consumers.Options;
using CrossCutting.Kafka.Common.Producers.Extensions;
using CrossCutting.Kafka.Common.Producers.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using User.Domain.Constants.RateLimitStrategy;
using User.Domain.Interfaces.Kafka.Producers.Register;
using User.Domain.Interfaces.Repositories.Base;
using User.Domain.Interfaces.Repositories.SellerApplication;
using User.Domain.Interfaces.Repositories.User;
using User.Domain.Interfaces.Repositories.UserProfile;
using User.Domain.Interfaces.Repositories.UserSession;
using User.Domain.Interfaces.Repositories.UserVerification;
using User.Domain.Interfaces.UoW;
using User.Infrastructure.Context;
using User.Infrastructure.Kafka.Envelopes.Register;
using User.Infrastructure.Kafka.Producers.Register;
using User.Infrastructure.Repositories.Base;
using User.Infrastructure.Repositories.SellerApplication;
using User.Infrastructure.Repositories.User;
using User.Infrastructure.Repositories.UserProfile;
using User.Infrastructure.Repositories.UserSession;
using User.Infrastructure.Repositories.UserVerification;
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
            .ConfigureKafkaServices(configuration)
            .AddProducerServices();
    }

    private static IServiceCollection ConfigureContextDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = Environment.GetEnvironmentVariable("USER_CONTEXT_DATA_SOURCE")
                                   ?? configuration.GetConnectionString("USER_CONTEXT_DATA_SOURCE")
                                   ?? throw new ArgumentNullException("Could not find the USER_CONTEXT_DATA_SOURCE environment variable");

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
        services.AddScoped<IUsersVerificationRepository, UsersVerificationRepository>();
        services.AddScoped<ISellersApplicationRepository, SellersApplicationRepository>();

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

        return services;
    }

    private static IServiceCollection AddProducers(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaProducerOptions>(configuration.GetSection("Kafka:Producer"));

        services.AddKafkaProducer<BidderUserCreatedEnvelope>(configuration);
        services.AddKafkaProducer<SellerUserCreatedEnvelope>(configuration);
        return services;
    }

    private static IServiceCollection AddProducerServices(this IServiceCollection services)
    {
        services.AddScoped<IRegisterBidderProducer, RegisterBidderProducer>();
        services.AddScoped<IRegisterSellerProducer, RegisterSellerProducer>();

        return services;
    }

    public static IServiceCollection ConfigureRateLimiters(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddFixedWindowLimiter(RateLimitStrategyConstant.Login_Fixed_Window, limiter =>
            {
                limiter.PermitLimit = 3;
                limiter.Window = TimeSpan.FromSeconds(15);
                limiter.QueueLimit = 0;
            });
        });

        return services;
    }
}
