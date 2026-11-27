using User.Domain.Dto.Profile.Response;
using User.Domain.Enums;

namespace User.UnitTests.Domain.DTO.Profile.Response;

public class ResponseUserProfileDTOTests
{
    [Theory]
    [InlineData(ERole.ADMIN)]
    [InlineData(ERole.BIDDER)]
    [InlineData(ERole.SELLER)]
    public void ResponseUserProfileDTO_Should_Have_Valid_Properties(ERole role)
    {
        string name = "name";
        string email = "email";
        string? bio = "bio";
        DateOnly birthDate = new(2000, 1, 1);
        var dto = new ResponseUserProfileDTO(name, email, bio, birthDate, role);
        Assert.Equal(name, dto.Name);
        Assert.Equal(email, dto.Email);
        Assert.Equal(bio, dto.Bio);
        Assert.Equal(birthDate, dto.BirthDate);
        Assert.Equal(role, dto.Role);
    }
}
