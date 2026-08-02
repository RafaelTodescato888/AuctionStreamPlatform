using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using User.Domain.Constants.Configuration;
using User.Domain.Entities;
using User.Domain.Enums;
using User.Domain.Interfaces.Services.Authentication.Register;

namespace User.Infrastructure.Seeds
{
    internal static class UsersProfileSeedExtensions
    {
        internal static DbContextOptionsBuilder UseAsyncUsersProfileSeeding(
            this DbContextOptionsBuilder dbContextOptionsBuilder,
            IServiceProvider serviceProvider)
        {
            dbContextOptionsBuilder
                .UseSeeding((context, _) =>
                {
                    if (!context.Set<Users>().Any(u => u.Role == ERole.ADMIN))
                    {
                        var adminConfigOptions = serviceProvider.GetRequiredService<IOptions<AdminConfig>>();

                        if (adminConfigOptions.Value == null)
                            throw new InvalidOperationException("Não foi possível encontrar a configuração padrão de Seed para Administrador do Sistema.");

                        var passwordHasher = serviceProvider.GetRequiredService<IPasswordHashService>();
                        var hashedPassword = passwordHasher.GenerateHash(adminConfigOptions.Value.Password);

                        var adminUser = new UsersProfile(adminConfigOptions.Value, hashedPassword);
                        context.Set<UsersProfile>().Add(adminUser);
                        context.SaveChanges();
                    }
                });

            return dbContextOptionsBuilder;
        }
    }
}
