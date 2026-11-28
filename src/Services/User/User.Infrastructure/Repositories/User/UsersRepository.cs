using Microsoft.EntityFrameworkCore;
using User.Domain.Dto.Login.Request;
using User.Domain.Dto.Register.Projections;
using User.Domain.Entities;
using User.Domain.Interfaces.Repositories.User;
using User.Infrastructure.Context;
using User.Infrastructure.Repositories.Base;

namespace User.Infrastructure.Repositories.User;

internal sealed class UsersRepository(UserContext context) : BaseRepository<Users>(context), IUsersRepository
{
    public Task<bool> ExistsByDocumentOrEmailAsync(string document, string email, CancellationToken cancellationToken = default)
    {
        return AnyAsync(u => u.Document == document || u.UsersProfile!.Email == email, cancellationToken);
    }

    public Task<UserCredentialsProjection?> GetUserByCredentialsAsync(RequestUserLoginDTO content, CancellationToken cancellationToken = default)
    {
        return GetAll(u => u.UsersProfile != null && u.UsersProfile.Email == content.Email)
            .Select(u => new UserCredentialsProjection(
                new RequestGenerateTokenDTO(u.Id, u.UsersProfile!.Name, u.Role, u.Status),
                u.HashPassword,
                u.FailedLoginAttempts))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> BlockUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null) return false;

        user.Block();

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> IncrementFailedLoginAttemptsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null) return false;

        user.IncrementFailedLoginAttempts();

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ResetLoginAttemptsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null) return false;

        user.ResetLoginAttemps();

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
