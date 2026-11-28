using User.Domain.Enums;

namespace User.Infrastructure.Kafka.Events;

public record BidderUserCreatedEvent(
    Guid UserId,
    string Email,
    EVerificationType VerificationType,
    string Code,
    DateTime ExpiresAt);
