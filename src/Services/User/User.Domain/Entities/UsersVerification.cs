using CrossCutting.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using User.Domain.Enums;

namespace User.Domain.Entities;

public class UsersVerification : BaseEntity
{
    public Guid UserId { get; private set; }
    public string CodeHash { get; private set; } = string.Empty;
    public EVerificationType Type { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public short Attempts { get; private set; }

    protected UsersVerification() { }

    public UsersVerification(
        Guid userId,
        string codeHash,
        EVerificationType type,
        DateTime expiresAt)
    {
        UserId = userId;
        CodeHash = codeHash;
        Type = type;
        ExpiresAt = expiresAt;
        Attempts = 0;
    }

    #region [Foreign Keys]
    [ForeignKey(nameof(UserId))]
    public Users User { get; private set; } = null!;
    #endregion

    #region [Factory]
    public static UsersVerification Create(
        Guid userId,
        EVerificationType verificationType,
        string codeHash,
        DateTime expiresAt)
    {
        return new UsersVerification(userId, codeHash, verificationType, expiresAt);
    }

    public void MarkAsUsed()
    {
        UsedAt = DateTime.UtcNow;
    }
    #endregion
}
