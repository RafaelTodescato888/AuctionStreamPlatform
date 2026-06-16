using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using User.Domain.Constants.Configuration;
using User.Domain.Dto.Profile;
using User.Domain.Dto.Register.Request;
using User.Domain.Entities;
using User.Domain.Interfaces.Repositories.UserProfile;
using User.Domain.Interfaces.Services.Caching;
using User.Infrastructure.Context;
using User.Infrastructure.Repositories.Base;

namespace User.Infrastructure.Repositories.UserProfile
{
    internal sealed class UsersProfileRepository(
        AuctionStreamPlatformContext context,
        ICachingService cachingService,
        IOptions<AppConfig> options) : BaseRepository<UsersProfile>(context), IUsersProfileRepository
    {
        public async Task<Guid?> CreateAsync(RequestRegisterUserDTO request, CancellationToken cancellationToken = default)
        {
            var userProfile = UsersProfile.Create(request);

            await context.UsersProfile.AddAsync(userProfile, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            return userProfile?.Id;
        }

        public async Task<UserProfileResponseDTO?> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"UserProfile_{id}";
            var cachedUser = cachingService.Get<UserProfileResponseDTO>(cacheKey);
            if (cachedUser != null)
                return cachedUser;

            var user = await GetAll(u => u.UserId == id)
                .Select(u => new UserProfileResponseDTO(u.Name, u.Email, u.Bio, u.BirthDate, u.User.Role))
                .FirstOrDefaultAsync(cancellationToken);

            if (user != null)
                cachingService.Set(cacheKey, user, TimeSpan.FromMilliseconds(options.Value.DefaultExpiration));

            return user;
        }
    }
}
