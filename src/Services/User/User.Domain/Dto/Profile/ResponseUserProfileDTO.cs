using User.Domain.Enums;

namespace User.Domain.Dto.Profile;

public record ResponseUserProfileDTO(string Name, string Email, string? Bio, DateOnly BirthDate, ERole Role);
