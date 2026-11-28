using CrossCutting.Errors.Base;

namespace User.Domain.Interfaces.UseCase.Login.Commands;

public interface ILoginWrongPasswordUseCase
{
    Task<BaseError> IncrementFailedLoginAttemptsOrBlockUserAsync(Guid userId, sbyte failedLoginAttempts, CancellationToken cancellationToken = default);
}
