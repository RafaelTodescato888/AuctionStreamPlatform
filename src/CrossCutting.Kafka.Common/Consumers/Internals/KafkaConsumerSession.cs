
using Confluent.Kafka;
using CrossCutting.Kafka.Common.Envelopes.Contracts;
using CrossCutting.Kafka.Common.Serialization;

namespace CrossCutting.Kafka.Common.Consumers.Internals;

internal sealed class KafkaConsumerSession<TEnvelope, TKey, TValue>
    : IKafkaConsumerSession<TEnvelope>
    where TEnvelope : IRetriableEnvelope<TKey, TValue>, new()
{
    private readonly IConsumer<TKey, TValue> _consumer;

    public KafkaConsumerSession(ConsumerConfig config)
    {
        var builder = new ConsumerBuilder<TKey, TValue>(config)
            .SetKeyDeserializer(
                KafkaDeserializerFactory.Create<TKey>())
            .SetValueDeserializer(
                KafkaDeserializerFactory.Create<TValue>());

        _consumer = builder.Build();
    }

    public void Subscribe(IEnumerable<string> topics)
    {
        _consumer.Subscribe(topics);
    }

    public KafkaConsumedMessage<TEnvelope>? Consume(
        CancellationToken cancellationToken)
    {
        var result = _consumer.Consume(cancellationToken);

        if (result is null)
            return null;

        var headers = result.Message.Headers
            .GroupBy(header => header.Key)
            .ToDictionary(
                group => group.Key,
                group => group.Last().GetValueBytes());

        var envelope = new TEnvelope
        {
            Key = result.Message.Key,
            Value = result.Message.Value,
            Headers = headers,
            IsRetrying = headers.ContainsKey("x-kafka-is-retrying")
        };

        return new KafkaConsumedMessage<TEnvelope>(
            envelope,
            result,
            result.Topic);
    }

    public void Commit(KafkaConsumedMessage<TEnvelope> message)
    {
        _consumer.Commit(
            (ConsumeResult<TKey, TValue>)message.NativeResult);
    }

    public void Close()
    {
        _consumer.Close();
    }
}
