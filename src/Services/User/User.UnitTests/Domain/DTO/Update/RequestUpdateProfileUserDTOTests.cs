using User.Domain.Dto.Update.Request;

namespace User.UnitTests.Domain.DTO.Update;

public class RequestUpdateProfileUserDTOTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("Software Developer")]
    public void RequestUpdateProfileUserDTO_Should_Have_Valid_Properties(string? bio)
    {
        string name = "John Doe";
        var dto = new RequestUpdateProfileUserDTO(name, bio);
        Assert.Equal(name, dto.Name);
        Assert.Equal(bio, dto.Bio);
    }
}
