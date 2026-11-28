using User.Domain.Dto.Login.Request;

namespace User.Domain.Dto.Register.Projections;

public sealed record UserCredentialsProjection(
    RequestGenerateTokenDTO User,
    string HashPassword,
    sbyte FailedLoginAttempts);
