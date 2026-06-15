using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using User.Domain.Constants.Configuration;
using User.Domain.Dto.Common;
using User.Domain.Dto.Login.Request;
using User.Domain.Interfaces.Services.Authentication.Login;

namespace User.Infrastructure.Services.Authentication.Login
{
    internal sealed class GenerateTokenService(
        IOptions<AppConfig> configurations
    ) : IGenerateTokenService
    {
        public RefreshTokenDTO GenerateRefreshToken()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            var refreshToken = Convert.ToBase64String(bytes);
            var expiratesAt = DateTime.UtcNow.AddDays(7);

            return new RefreshTokenDTO(refreshToken, expiratesAt);
        }

        public string GenerateToken(RequestGenerateTokenDTO content)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configurations.Value.JwtSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, content.Id.ToString()),
                new Claim(ClaimTypes.Name, content.Name),
                new Claim(ClaimTypes.Role, content.Role.ToString())
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
