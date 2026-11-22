namespace CrossCutting.Kafka.Common.Consumers.Internals;

internal interface IKafkaConsumerSession<TEnvelope>
{
    void Subscribe(IEnumerable<string> topics);

    KafkaConsumedMessage<TEnvelope>? Consume(
        CancellationToken cancellationToken);

    void Commit(KafkaConsumedMessage<TEnvelope> message);

    void Close();
}
