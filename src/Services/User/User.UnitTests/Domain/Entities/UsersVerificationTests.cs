using User.Domain.Entities;
using User.Domain.Enums;

namespace User.UnitTests.Domain.Entities;

public class UsersVerificationTests
{
    [Fact]
    public void UsersVerification_Should_Have_Valid_Properties_When_Created()
    {
        var userId = Guid.NewGuid();
        var codeHash = "hashed-verification-code";
        var type = EVerificationType.EMAIL_CONFIRMATION;
        var expiresAt = DateTime.UtcNow.AddMinutes(15);

        var verification = new UsersVerification(
            userId,
            codeHash,
            type,
            expiresAt
        );

        Assert.Equal(userId, verification.UserId);
        Assert.Equal(codeHash, verification.CodeHash);
        Assert.Equal(type, verification.Type);
        Assert.Equal(expiresAt, verification.ExpiresAt);
        Assert.Null(verification.UsedAt);
        Assert.Equal((short)0, verification.Attempts);
    }

    [Fact]
    public void UsersVerification_Should_Create_Verification_When_Create()
    {
        var userId = Guid.NewGuid();
        var codeHash = "hashed-verification-code";
        var type = EVerificationType.PASSWORD_RESET;
        var expiresAt = DateTime.UtcNow.AddMinutes(15);

        var verification = UsersVerification.Create(
            userId,
            type,
            codeHash,
            expiresAt
        );

        Assert.Equal(userId, verification.UserId);
        Assert.Equal(codeHash, verification.CodeHash);
        Assert.Equal(type, verification.Type);
        Assert.Equal(expiresAt, verification.ExpiresAt);
        Assert.Equal((short)0, verification.Attempts);
        Assert.Null(verification.UsedAt);
    }

    [Fact]
    public void UsersVerification_Should_Set_UsedAt_When_MarkAsUsed()
    {
        var verification = new UsersVerification(
            Guid.NewGuid(),
            "hashed-verification-code",
            EVerificationType.EMAIL_CONFIRMATION,
            DateTime.UtcNow.AddMinutes(15)
        );

        verification.MarkAsUsed();

        Assert.NotNull(verification.UsedAt);
        Assert.True(
            verification.UsedAt.Value <= DateTime.UtcNow
        );
    }
}