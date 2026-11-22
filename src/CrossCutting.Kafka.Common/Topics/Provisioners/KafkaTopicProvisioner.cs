using Confluent.Kafka;
using Confluent.Kafka.Admin;
using CrossCutting.Kafka.Common.Topics.Options;

namespace CrossCutting.Kafka.Common.Topics.Provisioners;

internal sealed class KafkaTopicProvisioner(
    IAdminClient adminClient,
    KafkaTopicOptions options)
    : IKafkaTopicProvisioner
{
    public async Task EnsureTopicsAsync(
        IEnumerable<string> topics,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!options.AutoCreate)
            return;

        var specifications = topics
            .Where(topic => !string.IsNullOrWhiteSpace(topic))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(topic => new TopicSpecification
            {
                Name = topic,
                NumPartitions = options.Partitions,
                ReplicationFactor = options.ReplicationFactor
            })
            .ToList();

        if (specifications.Count == 0)
            return;

        try
        {
            await adminClient.CreateTopicsAsync(specifications);
        }
        catch (CreateTopicsException ex)
        {
            var failures = ex.Results
                .Where(result =>
                    result.Error.Code != ErrorCode.TopicAlreadyExists)
                .ToList();

            if (failures.Count > 0)
                throw;
        }

        cancellationToken.ThrowIfCancellationRequested();
    }
}