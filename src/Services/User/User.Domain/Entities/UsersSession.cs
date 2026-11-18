using CrossCutting.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using User.Domain.Dto.Common;

namespace User.Domain.Entities;

public class UsersSession : BaseEntity
{
    public Guid UserId { get; private set; }
    public string RefreshTokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    protected UsersSession() { }

    public UsersSession(RefreshTokenDTO refreshToken, Guid userId)
    {
        UserId = userId;
        RefreshTokenHash = refreshToken.RefreshToken;
        ExpiresAt = refreshToken.ExpiresAt;
    }

    #region [Foreign Keys]
    [ForeignKey(nameof(UserId))]
    public Users User { get; private set; } = null!;
    #endregion

    #region [Factory]
    public static UsersSession Create(RefreshTokenDTO refreshToken, Guid userId)
    {
        return new UsersSession(refreshToken, userId);
    }

    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
    }
    #endregion
}
