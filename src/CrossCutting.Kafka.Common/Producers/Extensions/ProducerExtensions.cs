using Confluent.Kafka;
using CrossCutting.Kafka.Common.Envelopes.Contracts;
using CrossCutting.Kafka.Common.Producers.Contracts;
using CrossCutting.Kafka.Common.Producers.Internals;
using CrossCutting.Kafka.Common.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace CrossCutting.Kafka.Common.Producers.Extensions;

public static class ProducerExtensions
{
    public static IServiceCollection AddKafkaProducer<TEnvelope>(
        this IServiceCollection services,
        IConfiguration configuration)
        where TEnvelope : class, IEnvelope
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var producerConfig = configuration
            .GetSection("Kafka:Producer:Config")
            .Get<ProducerConfig>()
            ?? throw new InvalidOperationException(
                "Kafka producer configuration not found.");

        var envelopeInterface = ResolveEnvelopeInterface<TEnvelope>();

        var genericArguments = envelopeInterface.GetGenericArguments();

        var keyType = genericArguments[0];
        var valueType = genericArguments[1];

        RegisterKafkaProducer(
            services,
            keyType,
            valueType,
            producerConfig);

        RegisterKafkaProducerExecutor(
            services,
            typeof(TEnvelope),
            keyType,
            valueType);

        RegisterPublicProducer(
            services,
            typeof(TEnvelope));

        return services;
    }

    private static Type ResolveEnvelopeInterface<TEnvelope>()
    {
        var interfaces = typeof(TEnvelope)
            .GetInterfaces()
            .Where(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition() ==
                typeof(IRetriableEnvelope<,>))
            .ToArray();

        if (interfaces.Length != 1)
        {
            throw new InvalidOperationException(
                $"{typeof(TEnvelope).Name} must implement exactly one " +
                "IRetriableEnvelope<TKey, TValue>.");
        }

        return interfaces[0];
    }

    private static void RegisterKafkaProducer(
        IServiceCollection services,
        Type keyType,
        Type valueType,
        ProducerConfig config)
    {
        var producerType = typeof(IProducer<,>)
            .MakeGenericType(keyType, valueType);

        services.TryAddSingleton(
            producerType,
            _ => CreateProducer(keyType, valueType, config));
    }

    private static void RegisterKafkaProducerExecutor(
        IServiceCollection services,
        Type envelopeType,
        Type keyType,
        Type valueType)
    {
        var serviceType = typeof(IKafkaProducerExecutor<>)
            .MakeGenericType(envelopeType);

        var implementationType = typeof(KafkaProducerExecutor<,,>)
            .MakeGenericType(envelopeType, keyType, valueType);

        services.TryAddSingleton(serviceType, implementationType);
    }

    private static void RegisterPublicProducer(
        IServiceCollection services,
        Type envelopeType)
    {
        var serviceType = typeof(IProducer<>)
            .MakeGenericType(envelopeType);

        var implementationType = typeof(Producer<>)
            .MakeGenericType(envelopeType);

        services.TryAddSingleton(serviceType, implementationType);
    }

    private static object CreateProducer(
        Type keyType,
        Type valueType,
        ProducerConfig config)
    {
        var method = typeof(ProducerExtensions)
            .GetMethod(
                nameof(CreateProducerGeneric),
                BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(keyType, valueType);

        return method.Invoke(null, [config])!;
    }

    private static IProducer<TKey, TValue>
        CreateProducerGeneric<TKey, TValue>(
            ProducerConfig config)
    {
        return new ProducerBuilder<TKey, TValue>(config)
        .SetKeySerializer(KafkaSerializerFactory.Create<TKey>())
        .SetValueSerializer(KafkaSerializerFactory.Create<TValue>())
        .Build();
    }
}
