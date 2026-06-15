namespace User.Domain.Dto.Common
{
    public record RefreshTokenDTO(string RefreshToken, DateTime ExpiresAt);
}
