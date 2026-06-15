using CrossCutting.Errors.Base;
using Microsoft.AspNetCore.Http;

namespace User.Domain.Errors.Common
{
    public record UserNotFoundError() : BaseError("User not found.", nameof(UserNotFoundError), StatusCodes.Status404NotFound);
}
