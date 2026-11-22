namespace CrossCutting.Kafka.Common.Consumers.Internals;

internal sealed record KafkaConsumedMessage<TEnvelope>(
    TEnvelope Envelope,
    object NativeResult,
    string Topic);