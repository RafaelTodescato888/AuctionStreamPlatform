using CrossCutting.Kafka.Common.Envelopes.Contracts;

namespace CrossCutting.Kafka.Common.Producers.Contracts;

public interface IProducer<TEnvelope>
    where TEnvelope : IEnvelope
{
    Task ProduceAsync(
        TEnvelope envelope,
        CancellationToken cancellationToken = default);

    Task ProduceToFallBackAsync(
        TEnvelope envelope,
        CancellationToken cancellationToken = default);

    Task ProduceToRetryAsync(
        TEnvelope envelope,
        CancellationToken cancellationToken = default);

    Task ProduceToDeadLetterAsync(
        TEnvelope envelope,
        CancellationToken cancellationToken = default);

    Task ProduceBatchAsync(
        IEnumerable<TEnvelope> envelopes,
        CancellationToken cancellationToken = default);
}