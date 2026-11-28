using User.Domain.Dto.Register.Request;
using User.Domain.Enums;

namespace User.UnitTests.Domain.DTO.Register.Request;

public class RequestGenerateUserVerificationDTOTests
{
    [Theory]
    [InlineData(EVerificationType.CHANGE_EMAIL)]
    [InlineData(EVerificationType.EMAIL_CONFIRMATION)]
    [InlineData(EVerificationType.PASSWORD_RESET)]
    [InlineData(EVerificationType.PHONE_CONFIRMATION)]
    public void RequestGenerateUserVerificationDTO_Should_Have_Valid_Properties(EVerificationType verificationType)
    {
        var userId = Guid.NewGuid();
        var hashCode = "hash-code";
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var dto = new RequestGenerateUserVerificationDTO(userId, verificationType, hashCode, expiresAt);
        Assert.Equal(userId, dto.UserId);
        Assert.Equal(verificationType, dto.Type);
        Assert.Equal(hashCode, dto.HashCode);
        Assert.Equal(expiresAt, dto.ExpiresAt);
    }
}
