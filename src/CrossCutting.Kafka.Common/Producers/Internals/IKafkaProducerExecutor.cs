namespace CrossCutting.Kafka.Common.Producers.Internals;

public interface IKafkaProducerExecutor<TEnvelope>
{
    bool UseDeadLetterQueue(TEnvelope envelope);

    Task ProduceAsync(
        TEnvelope envelope,
        string topic,
        CancellationToken cancellationToken);
}
