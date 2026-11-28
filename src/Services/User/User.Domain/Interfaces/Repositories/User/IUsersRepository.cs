using User.Domain.Dto.Common;
using User.Domain.Dto.Login.Request;
using User.Domain.Entities;
using User.Domain.Interfaces.Repositories.Base;

namespace User.Domain.Interfaces.Repositories.User;

public interface IUsersRepository : IBaseRepository<Users>
{
    Task<bool> ExistsByDocumentOrEmailAsync(string document, string email, CancellationToken cancellationToken = default);
    Task<RequestGenerateTokenDTO?> GetUserByCredentialsAsync(RequestUserLoginDTO content, CancellationToken cancellationToken = default);
}
