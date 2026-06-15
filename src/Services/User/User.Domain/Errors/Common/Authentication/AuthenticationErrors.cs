using CrossCutting.Errors.Base;
using Microsoft.AspNetCore.Http;

namespace User.Domain.Errors.Common.Authentication
{
    public record UserIsNotAuthorizedError() : BaseError("User is not authorized to perform this action.", nameof(UserIsNotAuthorizedError), StatusCodes.Status403Forbidden);
}
