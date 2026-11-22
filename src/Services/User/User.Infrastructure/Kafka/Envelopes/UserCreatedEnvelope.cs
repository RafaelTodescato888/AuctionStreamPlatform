using CrossCutting.Kafka.Common.Enums;
using CrossCutting.Kafka.Common.Envelopes.Contracts;
using User.Domain.Constants.Topic;
using User.Infrastructure.Kafka.Events;

namespace User.Infrastructure.Kafka.Envelopes;

public class UserCreatedEnvelope : IRetriableEnvelope<string, UserCreatedEvent>
{
    public EMessageSerializer Serializer { get; set; } = EMessageSerializer.Json;
    public string GroupId { get; } = TopicConstants.GroupId;
    public string Key { get; set; } = null!;
    public UserCreatedEvent Value { get ; set ; } = null!;
    public bool UseDeadLetterQueue { get; } = true;

    public bool IsRetrying { get ; set ; }

    public string TopicName { get; } = TopicConstants.UserCreatedTopic;

    public IDictionary<string, byte[]> Headers { get; set; } = null!;
}
