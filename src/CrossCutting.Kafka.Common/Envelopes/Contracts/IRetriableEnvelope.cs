using CrossCutting.Kafka.Common.Enums;

namespace CrossCutting.Kafka.Common.Envelopes.Contracts;

public interface IRetriableEnvelope<TKey, TValue> : IEnvelope
{
    EMessageSerializer Serializer { get; set; }
    string GroupId { get; }
    TKey Key { get; set; }
    TValue Value { get; set; }
    bool UseDeadLetterQueue { get; }
    bool IsRetrying { get; set; }
}
