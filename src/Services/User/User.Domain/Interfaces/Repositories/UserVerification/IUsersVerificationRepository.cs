using User.Domain.Dto.Register.Request;
using User.Domain.Entities;
using User.Domain.Interfaces.Repositories.Base;

namespace User.Domain.Interfaces.Repositories.UserVerification;

public interface IUsersVerificationRepository : IBaseRepository<UsersVerification>
{
    Task<bool> UpsertAsync(RequestGenerateUserVerificationDTO request, CancellationToken cancellationToken = default);
}
