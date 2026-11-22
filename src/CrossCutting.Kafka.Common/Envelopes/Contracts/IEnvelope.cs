namespace CrossCutting.Kafka.Common.Envelopes.Contracts;

public interface IEnvelope
{
    string TopicName { get; }
    IDictionary<string, byte[]> Headers { get; set; }
}
