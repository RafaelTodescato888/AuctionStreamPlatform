using CrossCutting.Errors.Base;
using Microsoft.AspNetCore.Http;

namespace User.Domain.Errors.Login;

public record UserOrPasswordIsIncorrectError() : BaseError("The email or password provided is incorrect.", nameof(UserOrPasswordIsIncorrectError), StatusCodes.Status401Unauthorized);
public record UserIsBannedError() : BaseError("Current user is blocked.", nameof(UserIsBannedError), StatusCodes.Status423Locked);
public record UserIsOnRevisionError() : BaseError("Current user is on revision.", nameof(UserIsOnRevisionError), StatusCodes.Status403Forbidden);
