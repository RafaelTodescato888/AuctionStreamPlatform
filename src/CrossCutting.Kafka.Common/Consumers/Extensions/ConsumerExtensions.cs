using Confluent.Kafka;
using CrossCutting.Kafka.Common.Consumers.Base;
using CrossCutting.Kafka.Common.Consumers.Internals;
using CrossCutting.Kafka.Common.Envelopes.Contracts;
using CrossCutting.Kafka.Common.Topics.Options;
using CrossCutting.Kafka.Common.Topics.Provisioners;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CrossCutting.Kafka.Common.Consumers.Extensions;

public static class ConsumerExtensions
{
    public static IServiceCollection AddKafkaConsumer<
        TConsumer, TEnvelope>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TConsumer : ConsumerBase<TEnvelope>
        where TEnvelope : IEnvelope, new()
    {
        var config = configuration
            .GetSection("Kafka:Consumer:Config")
            .Get<ConsumerConfig>()
            ?? throw new InvalidOperationException(
                "Kafka consumer configuration was not found.");

        config.EnableAutoCommit = false;
        config.EnableAutoOffsetStore = false;

        var topicOptions = configuration
            .GetSection("Kafka:Topics")
            .Get<KafkaTopicOptions>()
            ?? new KafkaTopicOptions();

        services.TryAddSingleton<IKafkaConsumerFactory>(
            _ => new KafkaConsumerFactory(config));

        services.TryAddSingleton<IAdminClient>(_ =>
            new AdminClientBuilder(
                new AdminClientConfig
                {
                    BootstrapServers = config.BootstrapServers
                })
            .Build());

        services.TryAddSingleton<IKafkaTopicProvisioner>(
            provider => new KafkaTopicProvisioner(
                provider.GetRequiredService<IAdminClient>(),
                topicOptions));

        services.AddHostedService(provider =>
        {
            var consumer = ActivatorUtilities
                .CreateInstance<TConsumer>(provider);

            consumer.Initialize(
                provider.GetRequiredService<IKafkaConsumerFactory>(),
                provider.GetRequiredService<IKafkaTopicProvisioner>());

            return consumer;
        });

        return services;
    }
}