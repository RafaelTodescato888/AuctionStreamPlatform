using CrossCutting.Entities;
using Microsoft.EntityFrameworkCore;
using User.Domain.Entities;

namespace User.Infrastructure.Context
{
    internal sealed class AuctionStreamPlatformContext(DbContextOptions<AuctionStreamPlatformContext> options) : DbContext(options)
    {
        #region [Users]
        public DbSet<Users> Users { get; set; }
        public DbSet<UsersProfile> UsersProfile { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuctionStreamPlatformContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>()
                .Where(q => q.State is EntityState.Added or EntityState.Modified))
            {
                if (entry.State == EntityState.Modified)
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                else if (entry.State == EntityState.Added)
                    entry.Entity.CreatedAt ??= DateTime.UtcNow;
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
