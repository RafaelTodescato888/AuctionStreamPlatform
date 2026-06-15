using Microsoft.EntityFrameworkCore;
using User.Domain.Dto.Common;
using User.Domain.Dto.Login.Request;
using User.Domain.Entities;
using User.Domain.Interfaces.Repositories.User;
using User.Infrastructure.Context;
using User.Infrastructure.Repositories.Base;

namespace User.Infrastructure.Repositories.User
{
    internal sealed class UsersRepository(AuctionStreamPlatformContext context) : BaseRepository<Users>(context), IUsersRepository
    {
        public Task<bool> ExistsByDocumentAsync(string document, CancellationToken cancellationToken = default)
        {
            return AnyAsync(u => u.Document == document, cancellationToken);
        }

        public Task<RequestGenerateTokenDTO?> GetUserByCredentialsAsync(RequestUserLoginDTO content, CancellationToken cancellationToken = default)
        {
            return GetAll(u => u.UsersProfile != null && u.UsersProfile.Email == content.Email && u.HashPassword == content.Password)
                .Select(u => new RequestGenerateTokenDTO(u.Id, u.UsersProfile!.Name, u.Role, u.Status))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> RefreshTokenAsync(RefreshTokenDTO content, Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user == null)
                return false;

            user.SetRefreshToken(content);

            return await context.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
