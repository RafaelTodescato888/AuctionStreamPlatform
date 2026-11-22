namespace CrossCutting.Kafka.Common.Consumers.Options;

public class KafkaConsumerOptions
{
    public bool ConsumeRetryTopic { get; set; } = true;

    public string RetryTopicSuffix { get; set; } = "-retry";
}
