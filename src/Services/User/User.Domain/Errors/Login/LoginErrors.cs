using CrossCutting.Errors.Base;
using Microsoft.AspNetCore.Http;

namespace User.Domain.Errors.Login
{
    public record UserOrPasswordIsIncorrectError() : BaseError("The email or password provided is incorrect.", nameof(UserOrPasswordIsIncorrectError), StatusCodes.Status401Unauthorized);
    public record UserIsBannedError() : BaseError("Current user is permanently banned.", nameof(UserIsBannedError), StatusCodes.Status403Forbidden);
}
