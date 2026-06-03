using User.Domain.Entities;
using User.Domain.Interfaces.Repositories.User;
using User.Infrastructure.Context;
using User.Infrastructure.Repositories.Base;

namespace User.Infrastructure.Repositories.User
{
    internal sealed class UsersRepository(AuctionStreamPlatformContext context) : BaseRepository<Users>(context), IUsersRepository
    {
        public Task<bool> ExistsByDocumentAsync(string document, CancellationToken cancellationToken = default)
        {
            return AnyAsync(u => u.Document == document, cancellationToken);
        }
    }
}
