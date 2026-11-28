using User.Domain.Entities;

namespace User.UnitTests.Domain.Entities;

public class SellersApplicationTests
{
    [Fact]
    public void SellersApplication_Should_Have_Default_Properties()
    {
        var application = Activator.CreateInstance(
            typeof(SellersApplication),
            nonPublic: true
        ) as SellersApplication;

        Assert.NotNull(application);

        Assert.Equal(Guid.Empty, application.UserId);
        Assert.Null(application.ReviewedBy);
        Assert.Equal(default, application.Status);
        Assert.Null(application.ReviewComment);
        Assert.Equal(default, application.SubmittedAt);
        Assert.Null(application.ReviewedAt);
        Assert.Null(application.User);
        Assert.Null(application.ReviewedByUser);
    }
}