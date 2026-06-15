using User.Domain.Enums;

namespace User.Domain.Dto.Profile
{
    public record UserProfileResponseDTO(string Name, string Email, string? Bio, DateOnly BirthDate, ERole Role);
}
