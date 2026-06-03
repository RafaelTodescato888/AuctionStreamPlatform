using System.Text.Json.Serialization;

namespace User.Domain.Dto.Register.Request
{
    public record RequestRegisterUserDTO(
        string Name,
        string Email,
        string? Bio,
        DateOnly BirthDate,
        string Document,
        string Password
    )
    {
        public string Name { get; init; } = Name;
        public string Email { get; init; } = Email;
        public string? Bio { get; init; } = Bio;
        public DateOnly BirthDate { get; init; } = BirthDate;
        public string Document { get; init; } = Document;
        public string Password { get; init; } = Password;

        [JsonIgnore]
        public string HashedPassword { get; private set; } = string.Empty;

        public void SetHashedPassword(string hashedPassword)
        {
            HashedPassword = hashedPassword;
        }
    }
}
