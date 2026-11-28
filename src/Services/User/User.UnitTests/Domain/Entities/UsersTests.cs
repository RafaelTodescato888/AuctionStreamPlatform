using User.Domain.Entities;
using User.Domain.Enums;

namespace User.UnitTests.Domain.Entities;

public class UsersTests
{
    [Fact]
    public void Users_Should_Have_Valid_Properties_When_Created()
    {
        var document = "12345678900";
        var hashedPassword = "hashed-password";
        var role = ERole.ADMIN;

        var user = new Users(document, hashedPassword, role);

        Assert.Equal(document, user.Document);
        Assert.Equal(hashedPassword, user.HashPassword);
        Assert.Equal(role, user.Role);
        Assert.Equal(EUserStatus.ACTIVATION_PENDING, user.Status);
    }

    [Fact]
    public void Users_Should_Have_ReviewPending_Status_When_Created_As_Seller()
    {
        var document = "12345678900";
        var hashedPassword = "hashed-password";

        var user = new Users(document, hashedPassword, ERole.SELLER);

        Assert.Equal(document, user.Document);
        Assert.Equal(hashedPassword, user.HashPassword);
        Assert.Equal(ERole.SELLER, user.Role);
        Assert.Equal(EUserStatus.REVIEW_PENDING, user.Status);
    }

    [Theory]
    [InlineData(ERole.ADMIN)]
    [InlineData(ERole.BIDDER)]
    public void Users_Should_Have_Active_Status_When_Created_As_NonSeller(ERole role)
    {
        var user = new Users("12345678900", "hashed-password", role);

        Assert.Equal(EUserStatus.ACTIVATION_PENDING, user.Status);
    }

    [Fact]
    public void Users_Should_Create_Seller_When_CreateSeller()
    {
        var document = "12345678900";
        var hashedPassword = "hashed-password";

        var user = Users.CreateSeller(document, hashedPassword);

        Assert.Equal(document, user.Document);
        Assert.Equal(hashedPassword, user.HashPassword);
        Assert.Equal(ERole.SELLER, user.Role);
        Assert.Equal(EUserStatus.REVIEW_PENDING, user.Status);
    }
}