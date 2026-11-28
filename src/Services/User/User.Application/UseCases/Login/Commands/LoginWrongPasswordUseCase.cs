using CrossCutting.Errors.Base;
using Microsoft.Extensions.Options;
using User.Domain.Constants.Configuration;
using User.Domain.Errors.Login;
using User.Domain.Interfaces.UoW;
using User.Domain.Interfaces.UseCase.Login.Commands;

namespace User.Application.UseCases.Login.Commands;

public class LoginWrongPasswordUseCase(
    IOptions<LoginAttemptsConfig> options,
    IUnitOfWork unitOfWork
) : ILoginWrongPasswordUseCase
{
    public async Task<BaseError> IncrementFailedLoginAttemptsOrBlockUserAsync(Guid userId, sbyte failedLoginAttempts, CancellationToken cancellationToken = default)
    {
        if(options.Value.MaxFailedLoginAttempts - 1 <= failedLoginAttempts)
        {
            if (!await unitOfWork.UserRepository.BlockUserAsync(userId, cancellationToken))
                return new DatabaseError();
        }
        
        if (!await unitOfWork.UserRepository.IncrementFailedLoginAttemptsAsync(userId, cancellationToken))
            return new DatabaseError();
        
        
        return new UserOrPasswordIsIncorrectError();
    }
}
