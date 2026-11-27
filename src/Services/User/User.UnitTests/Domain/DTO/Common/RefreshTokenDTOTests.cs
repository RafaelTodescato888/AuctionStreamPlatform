using User.Domain.Dto.Common;

namespace User.UnitTests.Domain.DTO.Common;

public class RefreshTokenDTOTests
{
    [Fact]
    public void RefreshTokenDto_Should_Have_Properties()
    {
        var refreshToken = "sample-refresh-token";
        var expiresAt = DateTime.UtcNow.AddDays(7);

        var dto = new RefreshTokenDTO(refreshToken, expiresAt);

        Assert.Equal(refreshToken, dto.RefreshToken);
        Assert.Equal(expiresAt, dto.ExpiresAt);
    }
}
