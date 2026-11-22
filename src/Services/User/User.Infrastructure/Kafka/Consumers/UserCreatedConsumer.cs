using CrossCutting.Kafka.Common.Consumers.Base;
using CrossCutting.Kafka.Common.Producers.Contracts;
using Microsoft.Extensions.Logging;
using User.Infrastructure.Kafka.Envelopes;

namespace User.Infrastructure.Kafka.Consumers;

public class UserCreatedConsumer(
    ILogger<UserCreatedConsumer> logger,
    IProducer<UserCreatedEnvelope> producer
) : ConsumerBase<UserCreatedEnvelope>(logger)
{
    protected override Task HandleAsync(UserCreatedEnvelope envelope, CancellationToken cancellationToken)
    {
        logger.LogInformation("[{ClassName}] - User created: {UserId}", nameof(UserCreatedConsumer), envelope.Value.Id);

        return Task.CompletedTask;
    }

    protected override Task OnErrorAsync(UserCreatedEnvelope envelope, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "[{ClassName}] - Error occurred while processing user created event: {UserId}", nameof(UserCreatedConsumer), envelope.Value.Id);

        producer.ProduceToFallBackAsync(envelope, cancellationToken);

        return Task.CompletedTask;
    }
}
