using Microsoft.EntityFrameworkCore.Storage;
using User.Domain.Interfaces.Repositories.UserProfile;

namespace User.Domain.Interfaces.UoW
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        IUsersProfileRepository UsersProfileRepository { get; }
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct);
        Task<int> CommitAsync(CancellationToken ct);
    }
}
