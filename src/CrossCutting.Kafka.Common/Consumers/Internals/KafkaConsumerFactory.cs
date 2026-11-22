using Confluent.Kafka;
using CrossCutting.Kafka.Common.Envelopes.Contracts;
using System.Reflection;

namespace CrossCutting.Kafka.Common.Consumers.Internals;

internal sealed class KafkaConsumerFactory(
    ConsumerConfig config
) : IKafkaConsumerFactory
{
    public IKafkaConsumerSession<TEnvelope> Create<TEnvelope>()
        where TEnvelope : IEnvelope, new()
    {
        var envelopeInterface = typeof(TEnvelope)
            .GetInterfaces()
            .SingleOrDefault(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition() ==
                typeof(IRetriableEnvelope<,>));

        if (envelopeInterface is null)
        {
            throw new InvalidOperationException(
                $"{typeof(TEnvelope).Name} must implement " +
                $"{nameof(IRetriableEnvelope<object, object>)}.");
        }

        var metadata = new TEnvelope();

        var groupId = (string?)envelopeInterface
            .GetProperty(nameof(IRetriableEnvelope<object, object>.GroupId))!
            .GetValue(metadata);

        if (string.IsNullOrWhiteSpace(groupId))
        {
            throw new InvalidOperationException(
                $"GroupId not configured for {typeof(TEnvelope).Name}.");
        }

        var arguments = envelopeInterface.GenericTypeArguments;

        var method = GetType().GetMethod(
            nameof(CreateTyped),
            BindingFlags.NonPublic | BindingFlags.Instance)!;

        return (IKafkaConsumerSession<TEnvelope>)method
            .MakeGenericMethod(
                typeof(TEnvelope),
                arguments[0],
                arguments[1])
            .Invoke(this, [groupId])!;
    }

    private KafkaConsumerSession<TEnvelope, TKey, TValue> CreateTyped<
        TEnvelope, TKey, TValue>(string groupId)
        where TEnvelope : IRetriableEnvelope<TKey, TValue>, new()
    {
        var consumerConfig = new ConsumerConfig(config)
        {
            GroupId = groupId
        };

        return new KafkaConsumerSession<TEnvelope, TKey, TValue>(
            consumerConfig);
    }
}