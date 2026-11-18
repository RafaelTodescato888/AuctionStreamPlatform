using CrossCutting.Errors.Base;
using Microsoft.AspNetCore.Http;

namespace User.Domain.Errors.Register;

public record EmailIsRequiredError() : BaseError("Email is required.", nameof(BaseError), StatusCodes.Status400BadRequest);

public record BirthDateIsRequiredError() : BaseError("BirthDate is required.", nameof(BaseError), StatusCodes.Status400BadRequest);

public record DocumentIsRequiredError() : BaseError("Document is required.", nameof(BaseError), StatusCodes.Status400BadRequest);

public record DocumentIsInvalidError() : BaseError("Document is invalid.", nameof(BaseError), StatusCodes.Status400BadRequest);

public record PasswordIsRequiredError() : BaseError("Password is required.", nameof(BaseError), StatusCodes.Status400BadRequest);

public record UserAlreadyRegisteredError() : BaseError("User is already registered.", nameof(BaseError), StatusCodes.Status409Conflict);
