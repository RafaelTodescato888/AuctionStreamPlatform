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

    #region [Foreign Keys]
    [ForeignKey(nameof(UserId))]
    public Users User { get; private set; } = null!;
    #endregion
}
