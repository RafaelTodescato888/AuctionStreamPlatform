using User.Domain.Dto.Register.Response;
using User.Domain.Enums;

namespace User.UnitTests.Domain.DTO.Register.Response; 

public class ResponseGenerateUserVerificationDTOTests
{
    [Theory]
    [InlineData(EVerificationType.CHANGE_EMAIL)]
    [InlineData(EVerificationType.EMAIL_CONFIRMATION)]
    [InlineData(EVerificationType.PASSWORD_RESET)]
    [InlineData(EVerificationType.PHONE_CONFIRMATION)]
    public void ResponseGenerateUserVerificationDTO_Should_Have_Valid_Properties(EVerificationType verificationType)
    {
        Guid userId = Guid.CreateVersion7();
        string code = "123456";
        DateTime expiresAt = DateTime.UtcNow.AddHours(1);
        var dto = new ResponseGenerateUserVerificationDTO(userId, verificationType, code, expiresAt);
        Assert.Equal(userId, dto.UserId);
        Assert.Equal(verificationType, dto.VerificationType);
        Assert.Equal(code, dto.Code);
        Assert.Equal(expiresAt, dto.ExpiresAt);
    }
}
