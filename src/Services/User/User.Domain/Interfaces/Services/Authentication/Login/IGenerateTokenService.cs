using User.Domain.Dto.Common;
using User.Domain.Dto.Login.Request;

namespace User.Domain.Interfaces.Services.Authentication.Login
{
    public interface IGenerateTokenService
    {
        string GenerateToken(RequestGenerateTokenDTO content);
        RefreshTokenDTO GenerateRefreshToken();
    }
}
