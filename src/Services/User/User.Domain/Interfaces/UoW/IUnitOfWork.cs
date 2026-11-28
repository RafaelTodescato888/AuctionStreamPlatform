using Microsoft.EntityFrameworkCore.Storage;
using User.Domain.Interfaces.Repositories.SellerApplication;
using User.Domain.Interfaces.Repositories.User;
using User.Domain.Interfaces.Repositories.UserProfile;
using User.Domain.Interfaces.Repositories.UserSession;
using User.Domain.Interfaces.Repositories.UserVerification;

namespace User.Domain.Interfaces.UoW;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IUsersRepository UserRepository { get; }
    IUsersProfileRepository UsersProfileRepository { get; }
    IUsersSessionRepository UsersSessionRepository { get; }
    IUsersVerificationRepository UsersVerificationRepository { get; }
    ISellersApplicationRepository SellersApplicationRepository { get; }
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct);
    Task<int> CommitAsync(CancellationToken ct);
}
