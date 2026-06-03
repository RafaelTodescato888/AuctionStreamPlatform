using User.Domain.Dto.Register.Request;
using User.Domain.Entities;
using User.Domain.Interfaces.Repositories.UserProfile;
using User.Infrastructure.Context;
using User.Infrastructure.Repositories.Base;

namespace User.Infrastructure.Repositories.UserProfile
{
    internal sealed class UsersProfileRepository(AuctionStreamPlatformContext context) : BaseRepository<UsersProfile>(context), IUsersProfileRepository
    {
        public async Task<Guid?> CreateAsync(RequestRegisterUserDTO request, CancellationToken cancellationToken = default)
        {
            var userProfile = UsersProfile.Create(request);

            await context.UsersProfile.AddAsync(userProfile, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            return userProfile?.Id;
        }
    }
}
