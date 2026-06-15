using User.Domain.Dto.Common;
using User.Domain.Dto.Login.Request;
using User.Domain.Entities;
using User.Domain.Interfaces.Repositories.Base;

namespace User.Domain.Interfaces.Repositories.User
{
    public interface IUsersRepository : IBaseRepository<Users>
    {
        Task<bool> ExistsByDocumentAsync(string document, CancellationToken cancellationToken = default);
        Task<bool> RefreshTokenAsync(RefreshTokenDTO content, Guid userId, CancellationToken cancellationToken = default);
        Task<RequestGenerateTokenDTO?> GetUserByCredentialsAsync(RequestUserLoginDTO content, CancellationToken cancellationToken = default);
    }
}
