using User.Domain.Dto.Login.Request;
using User.Domain.Dto.Register.Projections;
using User.Domain.Entities;
using User.Domain.Interfaces.Repositories.Base;

namespace User.Domain.Interfaces.Repositories.User;

public interface IUsersRepository : IBaseRepository<Users>
{
    Task<bool> ExistsByDocumentOrEmailAsync(string document, string email, CancellationToken cancellationToken = default);
    Task<UserCredentialsProjection?> GetUserByCredentialsAsync(RequestUserLoginDTO content, CancellationToken cancellationToken = default);
    Task<bool> BlockUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IncrementFailedLoginAttemptsAsync(Guid userId, CancellationToken cancellationToken = default);
}
