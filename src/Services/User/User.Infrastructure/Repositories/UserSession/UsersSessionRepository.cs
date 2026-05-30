using Microsoft.EntityFrameworkCore;
using User.Domain.Dto.Common;
using User.Domain.Entities;
using User.Domain.Interfaces.Repositories.UserSession;
using User.Infrastructure.Context;
using User.Infrastructure.Repositories.Base;

namespace User.Infrastructure.Repositories.UserSession;

internal sealed class UsersSessionRepository(UserContext context) :
    BaseRepository<UsersSession>(context), IUsersSessionRepository
{
    public async Task<bool> UpsertAsync(RefreshTokenDTO refreshToken, Guid userId, CancellationToken cancellationToken = default)
    {
        var currentSession = await context.UsersSession
            .FirstOrDefaultAsync(s => s.UserId == userId && s.RevokedAt == null, cancellationToken);

        currentSession?.Revoke();

        var newSession = UsersSession.Create(refreshToken, userId);

        await context.UsersSession.AddAsync(newSession, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
