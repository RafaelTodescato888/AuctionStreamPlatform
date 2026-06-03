using Konscious.Security.Cryptography;
using Microsoft.Extensions.Options;
using System.Text;
using User.Domain.Constants.Configuration;
using User.Domain.Interfaces.Services.Authentication.Register;

namespace User.Infrastructure.Services.Authentication.Register
{
    internal sealed class PasswordHashService(
        IOptions<AppConfig> configurations
    ) : IPasswordHashService
    {
        public string GenerateHash(string password)
        {
            using var argon2 = GetConfiguredArgon(password);

            byte[] hashBytes = argon2.GetBytes(configurations.Value.MaxBytes);

            return Convert.ToBase64String(hashBytes);
        }

        private Argon2id GetConfiguredArgon(string password)
        {
            var passwordBytes = Encoding.UTF8.GetBytes(password);

            var salt = Encoding.UTF8.GetBytes(configurations.Value.Hash);

            using var argon2 = new Argon2id(passwordBytes);

            argon2.Salt = salt;
            argon2.DegreeOfParallelism = configurations.Value.LanesNumber;
            argon2.MemorySize = configurations.Value.MemorySize;
            argon2.Iterations = configurations.Value.Iterations;

            return argon2;
        }
    }
}
