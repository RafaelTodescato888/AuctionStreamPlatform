using CrossCutting.Kafka.Common.Envelopes.Contracts;
using CrossCutting.Kafka.Common.Producers.Constants;
using CrossCutting.Kafka.Common.Producers.Contracts;
using CrossCutting.Kafka.Common.Producers.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CrossCutting.Kafka.Common.Producers.Internals;

public sealed class Producer<TEnvelope>(
    IKafkaProducerExecutor<TEnvelope> executor,
    IOptions<KafkaProducerOptions> options,
    ILogger<Producer<TEnvelope>> logger
) : IProducer<TEnvelope>
    where TEnvelope : IEnvelope
{
    private readonly KafkaProducerOptions _options = options.Value;

    public Task ProduceAsync(
        TEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        return executor.ProduceAsync(
            envelope,
            envelope.TopicName,
            cancellationToken);
    }

    public async Task ProduceToFallBackAsync(
        TEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var retryCount = GetRetryCount(envelope.Headers);
        var nextRetryCount = retryCount + 1;

        if (envelope is not IRetriableEnvelope<object, object> &&
            !typeof(TEnvelope).GetInterfaces().Any(x =>
                x.IsGenericType &&
                x.GetGenericTypeDefinition() ==
                typeof(IRetriableEnvelope<,>)))
        {
            throw new InvalidOperationException(
                "Envelope must implement IRetriableEnvelope<TKey, TValue>.");
        }

        if (executor.UseDeadLetterQueue(envelope) &&
            nextRetryCount > _options.MaxRetryAttempts)
        {
            await ProduceToDeadLetterAsync(envelope, cancellationToken);
            return;
        }

        await ProduceToRetryAsync(envelope, cancellationToken);
    }

    public async Task ProduceToRetryAsync(
        TEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var retryCount = GetRetryCount(envelope.Headers) + 1;

        envelope.Headers[KafkaHeaderKeys.RetryCount] =
            BitConverter.GetBytes(retryCount);

        envelope.Headers[KafkaHeaderKeys.OriginalTopic] =
            System.Text.Encoding.UTF8.GetBytes(envelope.TopicName);

        envelope.Headers[KafkaHeaderKeys.IsRetrying] = [1];

        var topic = $"{envelope.TopicName}{_options.RetryTopicSuffix}";

        await executor.ProduceAsync(
            envelope,
            topic,
            cancellationToken);

        logger.LogWarning(
            "Message forwarded to retry topic. Topic: {Topic}, Retry: {RetryCount}",
            topic,
            retryCount);
    }

    public async Task ProduceToDeadLetterAsync(
        TEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var topic = $"{envelope.TopicName}{_options.DeadLetterTopicSuffix}";

        await executor.ProduceAsync(
            envelope,
            topic,
            cancellationToken);

        logger.LogError(
            "Message forwarded to DLQ. Topic: {Topic}",
            topic);
    }

    public async Task ProduceBatchAsync(
        IEnumerable<TEnvelope> envelopes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelopes);

        await Task.WhenAll(
            envelopes.Select(envelope =>
                ProduceAsync(envelope, cancellationToken)));
    }

    private static int GetRetryCount(
        IDictionary<string, byte[]>? headers)
    {
        if (headers is null ||
            !headers.TryGetValue(KafkaHeaderKeys.RetryCount, out var value))
        {
            return 0;
        }

        return value.Length == sizeof(int)
            ? BitConverter.ToInt32(value)
            : 0;
    }
}
