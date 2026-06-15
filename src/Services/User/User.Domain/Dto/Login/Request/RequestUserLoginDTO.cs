namespace User.Domain.Dto.Login.Request
{
    public class RequestUserLoginDTO(string Email, string Password)
    {
        public string Email { get; init; } = Email;
        public string Password { get; private set; } = Password;

        public void HashPassword(string hashedPassword)
        {
            Password = hashedPassword;
        }
    }
}
