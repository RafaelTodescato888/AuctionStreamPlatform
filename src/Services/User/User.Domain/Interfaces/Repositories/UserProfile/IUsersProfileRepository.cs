using User.Domain.Dto.Profile.Response;
using User.Domain.Dto.Register.Request;
using User.Domain.Dto.Update.Request;
using User.Domain.Entities;
using User.Domain.Enums;
using User.Domain.Interfaces.Repositories.Base;

namespace User.Domain.Interfaces.Repositories.UserProfile;

public interface IUsersProfileRepository : IBaseRepository<UsersProfile>
{
    Task<Guid?> CreateAsync(RequestRegisterUserDTO request, ERole role = ERole.BIDDER, CancellationToken cancellationToken = default);
    Task<ResponseUserProfileDTO?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(RequestUpdateProfileUserDTO request, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
