using Confluent.Kafka;
using CrossCutting.Kafka.Common.Envelopes.Contracts;
using Microsoft.Extensions.Logging;

namespace CrossCutting.Kafka.Common.Producers.Internals;

public sealed class KafkaProducerExecutor<TEnvelope, TKey, TValue>(
    IProducer<TKey, TValue> kafkaProducer,
    ILogger<KafkaProducerExecutor<TEnvelope, TKey, TValue>> logger
) : IKafkaProducerExecutor<TEnvelope>
    where TEnvelope : IRetriableEnvelope<TKey, TValue>
{
    public bool UseDeadLetterQueue(TEnvelope envelope)
    {
        return envelope.UseDeadLetterQueue;
    }

    public async Task ProduceAsync(
        TEnvelope envelope,
        string topic,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Kafka topic cannot be empty.");

        var message = new Message<TKey, TValue>
        {
            Key = envelope.Key,
            Value = envelope.Value,
            Headers = BuildHeaders(envelope.Headers),
        };

        try
        {
            var result = await kafkaProducer.ProduceAsync(
                topic,
                message,
                cancellationToken);

            logger.LogInformation(
                "Kafka message delivered. Topic: {Topic}, Partition: {Partition}, Offset: {Offset}",
                result.Topic,
                result.Partition.Value,
                result.Offset.Value);
        }
        catch (ProduceException<TKey, TValue> ex)
        {
            logger.LogError(
                ex,
                "Kafka delivery failed. Topic: {Topic}, Reason: {Reason}",
                topic,
                ex.Error.Reason);

            throw;
        }
    }

    private static Headers BuildHeaders(
        IDictionary<string, byte[]>? headers)
    {
        var kafkaHeaders = new Headers();

        if (headers is null)
            return kafkaHeaders;

        foreach (var header in headers)
            kafkaHeaders.Add(header.Key, header.Value);

        return kafkaHeaders;
    }
}