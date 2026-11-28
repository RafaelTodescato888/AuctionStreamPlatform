using User.Domain.Entities;

namespace User.UnitTests.Domain.Entities;

public class UsersRestrictionTests
{
    [Fact]
    public void UsersRestriction_Should_Have_Default_Properties()
    {
        var restriction = Activator.CreateInstance(
            typeof(UsersRestriction),
            nonPublic: true
        ) as UsersRestriction;

        Assert.NotNull(restriction);

        Assert.Equal(Guid.Empty, restriction.UserId);
        Assert.Null(restriction.CreatedBy);
        Assert.Equal(default, restriction.Type);
        Assert.Equal(string.Empty, restriction.Reason);
        Assert.Null(restriction.ExpiresAt);
        Assert.Null(restriction.User);
        Assert.Null(restriction.CreatedByUser);
    }
}