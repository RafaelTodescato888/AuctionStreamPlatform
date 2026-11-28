using User.Domain.Entities;
using User.Domain.Enums;
using User.Domain.Interfaces.Repositories.SellerApplication;
using User.Infrastructure.Context;
using User.Infrastructure.Repositories.Base;

namespace User.Infrastructure.Repositories.SellerApplication;

internal sealed class SellersApplicationRepository(UserContext context) :
    BaseRepository<SellersApplication>(context), ISellersApplicationRepository
{
    public async Task<Guid?> CreateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var sellerApplication = SellersApplication.Create(userId);

        await context.SellersApplications.AddAsync(sellerApplication, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return sellerApplication?.Id;
    }

    public Task<bool> ExistsByUserIdAsync(Guid userId, ESellerApplicationStatus status = ESellerApplicationStatus.PENDING, CancellationToken cancellationToken = default)
    {
        return AnyAsync(sa => sa.UserId == userId && sa.Status == status, cancellationToken);
    }
}
