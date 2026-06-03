using Microsoft.EntityFrameworkCore.Storage;
using User.Domain.Interfaces.Repositories.User;
using User.Domain.Interfaces.Repositories.UserProfile;
using User.Domain.Interfaces.UoW;
using User.Infrastructure.Context;

namespace User.Infrastructure.UoW
{
    internal sealed class UnitOfWork(
        AuctionStreamPlatformContext context,
        IUsersProfileRepository usersProfileRepository,
        IUsersRepository usersRepository
    ) : IUnitOfWork
    {
        public IUsersProfileRepository UsersProfileRepository { get; private set; } = usersProfileRepository;
        public IUsersRepository UserRepository { get; private set; } = usersRepository;

        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct)
        {
            return context.Database.BeginTransactionAsync(ct);
        }

        public Task<int> CommitAsync(CancellationToken ct)
        {
            return context.SaveChangesAsync(ct);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                context.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();
            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsyncCore()
        {
            await context.DisposeAsync();
        }
    }
}
