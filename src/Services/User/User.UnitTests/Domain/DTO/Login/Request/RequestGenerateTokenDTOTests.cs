using User.Domain.Dto.Login.Request;
using User.Domain.Enums;

namespace User.UnitTests.Domain.DTO.Login.Request;

public class RequestGenerateTokenDTOTests
{
    [Theory]
    [InlineData(ERole.ADMIN, EUserStatus.ACTIVE)]
    [InlineData(ERole.BIDDER, EUserStatus.ACTIVE)]
    [InlineData(ERole.SELLER, EUserStatus.ACTIVE)]
    [InlineData(ERole.BIDDER, EUserStatus.BANNED)]
    [InlineData(ERole.SELLER, EUserStatus.BANNED)]
    [InlineData(ERole.BIDDER, EUserStatus.REVIEW_PENDING)]
    [InlineData(ERole.SELLER, EUserStatus.REVIEW_PENDING)]
    [InlineData(ERole.BIDDER, EUserStatus.INACTIVE)]
    [InlineData(ERole.SELLER, EUserStatus.INACTIVE)]
    public void RequestGenerateTokenDTO_Should_Have_Properties(ERole role, EUserStatus status)
    {
        var id = Guid.NewGuid();
        var name = "John Doe";
        var dto = new RequestGenerateTokenDTO(id, name, role, status);
        Assert.Equal(id, dto.Id);
        Assert.Equal(name, dto.Name);
        Assert.Equal(role, dto.Role);
        Assert.Equal(status, dto.Status);
    }
}
