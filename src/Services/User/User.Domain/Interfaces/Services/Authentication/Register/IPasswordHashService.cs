namespace User.Domain.Interfaces.Services.Authentication.Register
{
    public interface IPasswordHashService
    {
        string GenerateHash(string password);
    }
}
