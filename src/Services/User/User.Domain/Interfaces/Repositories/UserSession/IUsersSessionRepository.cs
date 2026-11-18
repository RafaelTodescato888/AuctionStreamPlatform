using User.Domain.Dto.Common;
using User.Domain.Entities;
using User.Domain.Interfaces.Repositories.Base;

namespace User.Domain.Interfaces.Repositories.UserSession;

public interface IUsersSessionRepository : IBaseRepository<UsersSession>
{
    Task<bool> UpsertAsync(RefreshTokenDTO refreshToken, Guid userId, CancellationToken cancellationToken = default);
}
