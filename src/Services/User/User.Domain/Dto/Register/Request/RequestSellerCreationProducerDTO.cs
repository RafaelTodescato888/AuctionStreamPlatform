namespace User.Domain.Dto.Register.Request;

public record RequestSellerCreationProducerDTO(
    Guid UserId,
    Guid SellerApplicationId,
    DateTime OccurredAt,
    string Name,
    string Email
);
