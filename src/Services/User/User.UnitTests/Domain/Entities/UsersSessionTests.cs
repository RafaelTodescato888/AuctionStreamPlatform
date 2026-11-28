using User.Domain.Dto.Common;
using User.Domain.Entities;

namespace User.UnitTests.Domain.Entities;

public class UsersSessionTests
{
    [Fact]
    public void UsersSession_Should_Have_Valid_Properties_When_Created()
    {
        var userId = Guid.NewGuid();
        var refreshToken = new RefreshTokenDTO(
            "refresh-token-hash",
            DateTime.UtcNow.AddDays(7)
        );

        var session = new UsersSession(refreshToken, userId);

        Assert.Equal(userId, session.UserId);
        Assert.Equal(refreshToken.RefreshToken, session.RefreshTokenHash);
        Assert.Equal(refreshToken.ExpiresAt, session.ExpiresAt);
        Assert.Null(session.RevokedAt);
        Assert.Null(session.IpAddress);
        Assert.Null(session.UserAgent);
    }

    [Fact]
    public void UsersSession_Should_Create_Session_When_Create()
    {
        var userId = Guid.NewGuid();
        var refreshToken = new RefreshTokenDTO(
            "refresh-token-hash",
            DateTime.UtcNow.AddDays(7)
        );

        var session = UsersSession.Create(refreshToken, userId);

        Assert.Equal(userId, session.UserId);
        Assert.Equal(refreshToken.RefreshToken, session.RefreshTokenHash);
        Assert.Equal(refreshToken.ExpiresAt, session.ExpiresAt);
    }

    [Fact]
    public void UsersSession_Should_Set_RevokedAt_When_Revoke()
    {
        var userId = Guid.NewGuid();
        var refreshToken = new RefreshTokenDTO(
            "refresh-token-hash",
            DateTime.UtcNow.AddDays(7)
        );

        var session = new UsersSession(refreshToken, userId);

        session.Revoke();

        Assert.NotNull(session.RevokedAt);
        Assert.True(
            session.RevokedAt.Value <= DateTime.UtcNow
        );
    }
}