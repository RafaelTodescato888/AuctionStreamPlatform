using User.Domain.Enums;

namespace User.Domain.Dto.Register.Request;

public record RequestGenerateUserVerificationDTO(Guid UserId, EVerificationType Type, string HashCode, DateTime ExpiresAt);
