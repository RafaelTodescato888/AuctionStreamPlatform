using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using User.Domain.Entities;

namespace User.Infrastructure.Context.Configurations
{
    internal sealed class UsersConfigurations : IEntityTypeConfiguration<Users>
    {
        public void Configure(EntityTypeBuilder<Users> builder)
        {
            builder.Property(u => u.Document).HasMaxLength(11);
            builder.HasIndex(u => u.Document).IsUnique();
        }
    }
}
