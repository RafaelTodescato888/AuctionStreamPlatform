using User.Domain.Entities;
using User.Domain.Interfaces.Repositories.UserProfile;
using User.Infrastructure.Context;
using User.Infrastructure.Repositories.Base;

namespace User.Infrastructure.Repositories.UserProfile
{
    internal sealed class UsersProfileRepository(AuctionStreamPlatformContext context) : BaseRepository<UsersProfile>(context), IUsersProfileRepository
    {
    }
}
