namespace User.Infrastructure.Kafka.Events;

public record SellerUserCreatedEvent(
    Guid Id,
    DateTime OccurredAt,
    Guid UserId,
    Guid SellerApplicationId,
    string Name,
    string Email);
