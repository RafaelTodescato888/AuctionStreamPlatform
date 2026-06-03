using User.Domain.Dto.Register.Request;
using User.Domain.Entities;
using User.Domain.Interfaces.Repositories.Base;

namespace User.Domain.Interfaces.Repositories.UserProfile
{
    public interface IUsersProfileRepository : IBaseRepository<UsersProfile>
    {
        Task<Guid?> CreateAsync(RequestRegisterUserDTO request, CancellationToken cancellationToken = default);
    }
}
