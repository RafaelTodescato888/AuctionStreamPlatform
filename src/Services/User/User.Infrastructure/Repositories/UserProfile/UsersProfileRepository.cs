using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using User.Domain.Constants.Configuration;
using User.Domain.Dto.Profile;
using User.Domain.Dto.Register.Request;
using User.Domain.Dto.Update.Request;
using User.Domain.Entities;
using User.Domain.Enums;
using User.Domain.Interfaces.Repositories.UserProfile;
using User.Domain.Interfaces.Services.Caching;
using User.Infrastructure.Context;
using User.Infrastructure.Repositories.Base;

namespace User.Infrastructure.Repositories.UserProfile;

internal sealed class UsersProfileRepository(
    UserContext context,
    ICachingService cachingService,
    IOptions<AppConfig> options) : BaseRepository<UsersProfile>(context), IUsersProfileRepository
{
    public async Task<Guid?> CreateAsync(RequestRegisterUserDTO request, ERole role = ERole.BIDDER, CancellationToken cancellationToken = default)
    {
        var userProfile = UsersProfile.Create(request, role);

        await context.UsersProfile.AddAsync(userProfile, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return userProfile?.Id;
    }

    public Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return AnyAsync(up => up.UserId == userId, cancellationToken);
    }

    public async Task<ResponseUserProfileDTO?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"UserProfile_{id}";
        var cachedUser = cachingService.Get<ResponseUserProfileDTO>(cacheKey);
        if (cachedUser != null)
            return cachedUser;

        var user = await GetAll(u => u.UserId == id)
            .Select(u => new ResponseUserProfileDTO(u.Name, u.Email, u.Bio, u.BirthDate, u.User.Role))
            .FirstOrDefaultAsync(cancellationToken);

        if (user != null)
            cachingService.Set(cacheKey, user, TimeSpan.FromMilliseconds(options.Value.DefaultExpiration));

        return user;
    }

    public async Task<bool> UpdateAsync(RequestUpdateProfileUserDTO request, Guid userId, CancellationToken cancellationToken = default)
    {
        var userProfileToUpdate = await FirstOrDefaultAsync(up => up.UserId == userId, cancellationToken);
        if (userProfileToUpdate == null)
            return false;

        userProfileToUpdate.Update(request);
        await context.SaveChangesAsync(cancellationToken);

        cachingService.Remove($"UserProfile_{userId}");
        return true;
    }
}
