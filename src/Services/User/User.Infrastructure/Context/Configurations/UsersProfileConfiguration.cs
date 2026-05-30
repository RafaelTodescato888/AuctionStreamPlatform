using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using User.Domain.Entities;

namespace User.Infrastructure.Context.Configurations
{
    internal sealed class UsersProfileConfiguration : IEntityTypeConfiguration<UsersProfile>
    {
        public void Configure(EntityTypeBuilder<UsersProfile> builder)
        {
            builder.Property(y => y.Name).HasMaxLength(50);

            builder.Property(y => y.Email).HasMaxLength(100);
            builder.HasIndex(y => y.Email);

            builder.Property(y => y.Bio).HasMaxLength(150);
        }
    }
}
