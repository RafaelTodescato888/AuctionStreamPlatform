using CrossCutting.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using User.Domain.Enums;

namespace User.Domain.Entities;

public class UsersRestriction : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public ERestrictionType Type { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTime? ExpiresAt { get; private set; }

    #region [Foreign Keys]
    [ForeignKey(nameof(UserId))]
    public Users User { get; private set; } = null!;

    [ForeignKey(nameof(CreatedBy))]
    public Users? CreatedByUser { get; private set; }
    #endregion
}
