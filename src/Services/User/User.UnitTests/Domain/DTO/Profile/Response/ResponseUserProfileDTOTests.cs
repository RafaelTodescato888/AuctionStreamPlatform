using User.Domain.Dto.Profile.Response;
using User.Domain.Enums;

namespace User.UnitTests.Domain.DTO.Profile.Response;

public class ResponseUserProfileDTOTests
{
    [Theory]
    [InlineData(ERole.ADMIN, EUserStatus.ACTIVE)]
    [InlineData(ERole.BIDDER, EUserStatus.ACTIVE)]
    [InlineData(ERole.SELLER, EUserStatus.ACTIVE)]
    [InlineData(ERole.BIDDER, EUserStatus.BANNED)]
    [InlineData(ERole.SELLER, EUserStatus.BANNED)]
    [InlineData(ERole.BIDDER, EUserStatus.TEMPORARILY_BLOCKED)]
    [InlineData(ERole.SELLER, EUserStatus.TEMPORARILY_BLOCKED)]
    [InlineData(ERole.BIDDER, EUserStatus.ACTIVATION_PENDING)]
    [InlineData(ERole.SELLER, EUserStatus.REVIEW_PENDING)]
    public void ResponseUserProfileDTO_Should_Have_Valid_Properties(ERole role, EUserStatus status)
    {
        string name = "name";
        string email = "email";
        string? bio = "bio";
        DateOnly birthDate = new(2000, 1, 1);
        var dto = new ResponseUserProfileDTO(name, email, bio, birthDate, role, status);
        Assert.Equal(name, dto.Name);
        Assert.Equal(email, dto.Email);
        Assert.Equal(bio, dto.Bio);
        Assert.Equal(birthDate, dto.BirthDate);
        Assert.Equal(role, dto.Role);
    }
}
