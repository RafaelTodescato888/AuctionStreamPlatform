namespace CrossCutting.Kafka.Common.Topics.Provisioners;

public interface IKafkaTopicProvisioner
{
    Task EnsureTopicsAsync(
        IEnumerable<string> topics,
        CancellationToken cancellationToken = default);
}