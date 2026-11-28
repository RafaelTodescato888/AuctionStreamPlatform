using System;
using System.Collections.Generic;
using System.Text;
using User.Domain.Dto.Register.Request;
using User.Domain.Entities;
using User.Domain.Enums;
using User.Domain.Interfaces.Repositories.UserVerification;
using User.Infrastructure.Context;
using User.Infrastructure.Repositories.Base;

namespace User.Infrastructure.Repositories.UserVerification;

internal sealed class UsersVerificationRepository(UserContext context) :
    BaseRepository<UsersVerification>(context), IUsersVerificationRepository
{
    public async Task<bool> UpsertAsync(RequestGenerateUserVerificationDTO request, CancellationToken cancellationToken = default)
    {
        var currentVerification = context.UsersVerifications
            .FirstOrDefault(v => v.UserId == request.UserId && v.Type == request.Type && v.UsedAt == null);

        currentVerification?.MarkAsUsed();

        var newVerification = UsersVerification.Create(
            request.UserId,
            request.Type,
            request.HashCode,
            request.ExpiresAt);

        await context.UsersVerifications.AddAsync(newVerification, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
