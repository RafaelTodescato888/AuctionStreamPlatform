using User.Domain.Dto.Login.Response;

namespace User.UnitTests.Domain.DTO.Login.Response;

public class ResponseUserLoginDTOTests
{
    [Fact]
    public void ResponseUserLoginDTO_Should_Have_Valid_Properties()
    {
        string token = "token";
        string refreshToken = "refresh-token";

        var dto = new ResponseUserLoginDTO(token, refreshToken);

        Assert.Equal(token, dto.Token);
        Assert.Equal(refreshToken, dto.RefreshToken);
    }
}
