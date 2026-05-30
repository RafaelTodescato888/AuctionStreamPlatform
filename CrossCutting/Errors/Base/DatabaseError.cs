using Microsoft.AspNetCore.Http;

namespace CrossCutting.Errors.Base
{
    public record DatabaseError() : BaseError("An internal error occurred. Try again later", nameof(DatabaseError), StatusCodes.Status500InternalServerError);
}
