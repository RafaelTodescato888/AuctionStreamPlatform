using User.Domain.Entities;
using User.Domain.Interfaces.Repositories.Base;

namespace User.Domain.Interfaces.Repositories.User
{
    public interface IUsersRepository : IBaseRepository<Users>
    {
        Task<bool> ExistsByDocumentAsync(string document, CancellationToken cancellationToken = default);
    }
}
