using User.Domain.Enums;

namespace User.Domain.Dto.Register.Response;

public record ResponseGenerateUserVerificationDTO(
    Guid UserId,
    EVerificationType VerificationType,
    string Code,
    DateTime ExpiresAt);