using CrossCutting.Kafka.Common.Envelopes.Contracts;

namespace CrossCutting.Kafka.Common.Consumers.Internals;

internal interface IKafkaConsumerFactory
{
    IKafkaConsumerSession<TEnvelope> Create<TEnvelope>()
        where TEnvelope : IEnvelope, new();
}
