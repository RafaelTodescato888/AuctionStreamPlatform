using User.Domain.Entities;

namespace User.UnitTests.Domain.Entities;

public class UsersReputationTests
{
    [Fact]
    public void UsersReputation_Should_Have_Default_Properties()
    {
        var reputation = Activator.CreateInstance(
            typeof(UsersReputation),
            nonPublic: true
        ) as UsersReputation;

        Assert.NotNull(reputation);

        Assert.Equal(Guid.Empty, reputation.UserId);
        Assert.Equal(0, reputation.Score);
        Assert.Equal(0, reputation.AuctionsWon);
        Assert.Equal(0, reputation.AuctionsParticipated);
        Assert.Equal(0, reputation.SuccessfulPayments);
        Assert.Equal(0, reputation.FailedPayments);
        Assert.Null(reputation.User);
    }
}