using CrossCutting.Kafka.Common.Consumers.Internals;
using CrossCutting.Kafka.Common.Consumers.Options;
using CrossCutting.Kafka.Common.Envelopes.Contracts;
using CrossCutting.Kafka.Common.Topics.Provisioners;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CrossCutting.Kafka.Common.Consumers.Base;

public abstract class ConsumerBase<TEnvelope>(
    ILogger? logger = null,
    IOptions<KafkaConsumerOptions>? options = null
) : BackgroundService
    where TEnvelope : IEnvelope, new()
{
    private IKafkaConsumerSession<TEnvelope>? _session;
    private IKafkaTopicProvisioner? _topicProvisioner;

    private readonly KafkaConsumerOptions _options =
        options?.Value ?? new KafkaConsumerOptions();

    internal void Initialize(IKafkaConsumerFactory factory, IKafkaTopicProvisioner topicProvisioner)
    {
        _session = factory.Create<TEnvelope>();
        _topicProvisioner = topicProvisioner;
    }

    protected abstract Task HandleAsync(
        TEnvelope envelope,
        CancellationToken cancellationToken);

    protected abstract Task OnErrorAsync(
        TEnvelope envelope,
        Exception exception,
        CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        if (_session is null)
        {
            throw new InvalidOperationException(
                "Kafka consumer was not initialized.");
        }

        if (_topicProvisioner is null)
        {
            throw new InvalidOperationException(
                "Kafka topic provisioner was not initialized.");
        }
        var metadata = new TEnvelope();

        var topics = BuildTopics(metadata);

        await _topicProvisioner.EnsureTopicsAsync(topics, stoppingToken);

        _session.Subscribe(topics);

        logger?.LogInformation(
            "Kafka consumer started. Topics: {Topics}",
            string.Join(", ", topics));

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                KafkaConsumedMessage<TEnvelope>? message;

                try
                {
                    message = _session.Consume(stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Confluent.Kafka.ConsumeException ex)
                {
                    logger?.LogError(
                        ex,
                        "Kafka consume error: {Reason}",
                        ex.Error.Reason);

                    continue;
                }

                if (message is null)
                    continue;

                try
                {
                    await HandleAsync(
                        message.Envelope,
                        stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger?.LogError(
                        ex,
                        "Error processing Kafka message. Topic: {Topic}",
                        message.Topic);

                    await OnErrorAsync(
                        message.Envelope,
                        ex,
                        stoppingToken);
                }

                _session.Commit(message);
            }
        }
        finally
        {
            _session.Close();
        }
    }

    private IReadOnlyCollection<string> BuildTopics(TEnvelope metadata)
    {
        var topics = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            metadata.TopicName
        };

        if (_options.ConsumeRetryTopic)
        {
            topics.Add(
                $"{metadata.TopicName}{_options.RetryTopicSuffix}");
        }

        return topics;
    }
}
