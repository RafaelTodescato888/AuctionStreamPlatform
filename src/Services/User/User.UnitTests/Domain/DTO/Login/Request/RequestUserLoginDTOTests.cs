using User.Domain.Dto.Login.Request;

namespace User.UnitTests.Domain.DTO.Login.Request;

public class RequestUserLoginDTOTests
{
    [Fact]
    public void RequestUserLoginDTO_Should_Have_Valid_Properties()
    {
        string email = "";
        string password = "password";

        var dto = new RequestUserLoginDTO(email, password);

        Assert.Equal(email, dto.Email);
        Assert.Equal(password, dto.Password);
    }

    [Fact]
    public void RequestUserLoginDTO_Should_Update_When_HashPassword()
    {
        var email = "email";
        var oldPassword = "old-password";
        var hashedPassword = "hashed-password";

        var dto = new RequestUserLoginDTO(email, oldPassword);

        dto.HashPassword(hashedPassword);

        Assert.Equal(hashedPassword, dto.Password);
        Assert.Equal(email, dto.Email);
    }
}
