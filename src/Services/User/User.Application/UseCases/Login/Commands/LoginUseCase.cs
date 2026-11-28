using CrossCutting.Errors.Base;
using CrossCutting.Extensions;
using Microsoft.Extensions.Logging;
using OneOf;
using System.Diagnostics;
using User.Application.UseCases.Register.Seller.Commands;
using User.Domain.Constants.BusinessDomain;
using User.Domain.Constants.Service;
using User.Domain.Dto.Login.Request;
using User.Domain.Dto.Login.Response;
using User.Domain.Errors.Login;
using User.Domain.Interfaces.Services.Authentication.Login;
using User.Domain.Interfaces.Services.Authentication.Register;
using User.Domain.Interfaces.UoW;
using User.Domain.Interfaces.UseCase.Login.Commands;

namespace User.Application.UseCases.Login.Commands;

internal sealed class LoginUseCase(
    IUnitOfWork unitOfWork,
    IPasswordHashService passwordHashService,
    IGenerateTokenService generateTokenService,
    ILoginWrongPasswordUseCase loginWrongPasswordUseCase,
    ILogger<LoginUseCase> logger
) : ILoginUseCase
{
    public async Task<OneOf<ResponseUserLoginDTO, BaseError>> LoginAsync(RequestUserLoginDTO content, CancellationToken cancellationToken = default)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        logger.LogInformation("Executing [{ClassName}] on [{ServiceName}] User {Email} is trying to login", nameof(LoginUseCase), ServiceNameConstants.Name, content.Email);
        var hashPassword = passwordHashService.GenerateHash(content.Password);
        content.HashPassword(hashPassword);

        var userToLogin = await GetUserByCredentials(content, cancellationToken);
        if (userToLogin.IsError())
        {
            logger.LogInformation("Executing [{ClassName}] on [{ServiceName}] Failed validation: {ValidationError} for user {Email}, ErrorCode {ErrorCode}",
                nameof(LoginUseCase),
                ServiceNameConstants.Name,
                userToLogin.GetError().Message,
                content.Email,
                userToLogin.GetError().HttpErrorCode);

            return userToLogin.GetError();
        }

        var userToLoginInformations = userToLogin.GetValue();
        var refreshTokenInformations = generateTokenService.GenerateRefreshToken();

        if (!await unitOfWork.UsersSessionRepository.UpsertAsync(refreshTokenInformations, userToLoginInformations.Id, cancellationToken))
        {
            var error = new DatabaseError();
            logger.LogInformation("Executing [{ClassName}] on [{ServiceName}] during Session Generation, ErrorCode {ErrorCode}",
                nameof(LoginUseCase),
                ServiceNameConstants.Name,
                error.HttpErrorCode);

            return error;
        }

        stopwatch.Stop();
        var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        logger.LogInformation("Executed [{ClassName}] on [{ServiceName}] User: [Id: {Id}, Name: {Name}, Role: {Role}] logged successfully in {ElapsedMilliseconds} ms",
            nameof(RegisterSellerUseCase),
            ServiceNameConstants.Name,
            userToLoginInformations.Id,
            userToLoginInformations.Name,
            userToLoginInformations.Role.ToString(),
            elapsedMilliseconds);

        return new ResponseUserLoginDTO(generateTokenService.GenerateToken(userToLoginInformations), refreshTokenInformations.RefreshToken);
    }

    private async Task<OneOf<RequestGenerateTokenDTO, BaseError>> GetUserByCredentials(RequestUserLoginDTO content, CancellationToken cancellationToken = default)
    {
        var userToLogin = await unitOfWork.UserRepository.GetUserByCredentialsAsync(content, cancellationToken);
        if (userToLogin == null)
            return new UserOrPasswordIsIncorrectError();

        if (UserConstants.BlockedUserStatuses.Contains(userToLogin.User.Status))
            return new UserIsBannedError();

        if (UserConstants.ByRevisionUserStatuses.Contains(userToLogin.User.Status))
            return new UserIsOnRevisionError();

        if (userToLogin.HashPassword != content.Password)
        {
            return await loginWrongPasswordUseCase.IncrementFailedLoginAttemptsOrBlockUserAsync(
                userToLogin.User.Id,
                userToLogin.FailedLoginAttempts,
                cancellationToken);
        }

        return userToLogin.User;
    }
}
