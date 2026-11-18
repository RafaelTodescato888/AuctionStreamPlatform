using CrossCutting.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace User.Domain.Entities;

public class UsersReputation : BaseEntity
{
    public Guid UserId { get; private set; }
    public int Score { get; private set; }
    public int AuctionsWon { get; private set; }
    public int AuctionsParticipated { get; private set; }
    public int SuccessfulPayments { get; private set; }
    public int FailedPayments { get; private set; }

    #region [Foreign Keys]
    [ForeignKey(nameof(UserId))]
    public Users User { get; private set; } = null!;
    #endregion
}
