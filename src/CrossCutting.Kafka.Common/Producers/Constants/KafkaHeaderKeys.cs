namespace CrossCutting.Kafka.Common.Producers.Constants;

public static class KafkaHeaderKeys
{
    public const string RetryCount = "x-kafka-retry-count";
    public const string OriginalTopic = "x-kafka-original-topic";
    public const string LastError = "x-kafka-last-error";
    public const string IsRetrying = "x-kafka-is-retrying";
}
