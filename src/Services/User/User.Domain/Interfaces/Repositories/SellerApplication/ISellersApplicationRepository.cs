using User.Domain.Entities;
using User.Domain.Enums;
using User.Domain.Interfaces.Repositories.Base;

namespace User.Domain.Interfaces.Repositories.SellerApplication;

public interface ISellersApplicationRepository : IBaseRepository<SellersApplication>
{
    Task<bool> ExistsByUserIdAsync(Guid userId, ESellerApplicationStatus status = ESellerApplicationStatus.PENDING, CancellationToken cancellationToken = default);
    Task<Guid?> CreateAsync(Guid userId, CancellationToken cancellationToken = default);
}
