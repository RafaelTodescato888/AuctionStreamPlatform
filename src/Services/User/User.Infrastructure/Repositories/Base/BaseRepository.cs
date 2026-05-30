using System.Linq.Expressions;
using User.Domain.Interfaces.Repositories.Base;
using User.Infrastructure.Context;

namespace User.Infrastructure.Repositories.Base
{
    internal class BaseRepository<TEntity>(AuctionStreamPlatformContext context) : IBaseRepository<TEntity> where TEntity : class
    {
        public IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>> predicate)
        {
            return context.Set<TEntity>().Where(predicate);
        }

        public IQueryable<TEntity> GetAll()
        {
            return context.Set<TEntity>().AsQueryable();
        }
    }
}
