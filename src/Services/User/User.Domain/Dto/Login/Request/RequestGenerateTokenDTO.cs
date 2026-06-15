using User.Domain.Enums;

namespace User.Domain.Dto.Login.Request
{
    public record RequestGenerateTokenDTO(Guid Id, string Name, ERole Role, EUserStatus Status);
}
