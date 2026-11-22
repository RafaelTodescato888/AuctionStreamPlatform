namespace CrossCutting.Kafka.Common.Topics.Options;

public sealed class KafkaTopicOptions
{
    public bool AutoCreate { get; set; } = true;

    public int Partitions { get; set; } = 3;

    public short ReplicationFactor { get; set; } = 1;
}
