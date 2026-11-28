using CrossCutting.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using User.Domain.Enums;

namespace User.Domain.Entities;

public class SellersApplication : BaseEntity
{
    public Guid UserId { get; private set; }
    public Guid? ReviewedBy { get; private set; }
    public ESellerApplicationStatus Status { get; private set; }
    public string? ReviewComment { get; private set; }
    public DateTime SubmittedAt { get; private set; }
    public DateTime? ReviewedAt { get; private set; }

    #region [Foreign Keys]
    [ForeignKey(nameof(UserId))]
    public Users User { get; private set; } = null!;

    [ForeignKey(nameof(ReviewedBy))]
    public Users? ReviewedByUser { get; private set; }
    #endregion

    #region [Factory]
    public static SellersApplication Create(Guid userId)
    {
        return new SellersApplication
        {
            UserId = userId,
            Status = ESellerApplicationStatus.PENDING,
            SubmittedAt = DateTime.UtcNow
        };
    }
    #endregion
}
