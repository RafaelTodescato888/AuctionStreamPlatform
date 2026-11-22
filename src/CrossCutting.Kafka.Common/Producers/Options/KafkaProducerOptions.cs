namespace CrossCutting.Kafka.Common.Producers.Options;

public class KafkaProducerOptions
{
    public string RetryTopicSuffix { get; set; } = "-retry";
    public string DeadLetterTopicSuffix { get; set; } = "-dlq";
    public int MaxRetryAttempts { get; set; } = 3;
}
