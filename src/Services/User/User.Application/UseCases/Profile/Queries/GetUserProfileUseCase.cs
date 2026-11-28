using CrossCutting.Errors.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OneOf;
using System.Diagnostics;
using User.Domain.Constants.BusinessDomain;
using User.Domain.Constants.Service;
using User.Domain.Dto.Profile.Response;
using User.Domain.Errors.Common;
using User.Domain.Errors.Common.Authentication;
using User.Domain.Errors.Login;
using User.Domain.Helpers;
using User.Domain.Interfaces.UoW;
using User.Domain.Interfaces.UseCase.Profile.Queries;

namespace User.Application.UseCases.Profile.Queries;

internal sealed class GetUserProfileUseCase(
    IHttpContextAccessor httpContextAccessor,
    IUnitOfWork unitOfWork,
    ILogger<GetUserProfileUseCase> logger
) : IGetUserProfileUseCase
{
    public async Task<OneOf<ResponseUserProfileDTO, BaseError>> GetAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        logger.LogInformation("[{ClassName}] Getting user profile", nameof(GetUserProfileUseCase));
        var userId = SessionHelper.GetUserId(httpContextAccessor.HttpContext);

        if (!userId.HasValue)
        {
            var error = new UserIsNotAuthorizedError();

            logger.LogWarning("[{ClassName}] on [{ServiceName}] User is not authorized, ErrorCode: {ErrorCode}", nameof(GetUserProfileUseCase), ServiceNameConstants.Name, error.HttpErrorCode);
            return error;
        }

        var user = await unitOfWork.UsersProfileRepository.GetAsync(userId.Value, cancellationToken);
        if (user == null)
        {
            var error = new UserNotFoundError();

            logger.LogWarning("[{ClassName}] on [{ServiceName}] User with ID {UserId} not found, ErrorCode: {ErrorCode}", nameof(GetUserProfileUseCase), ServiceNameConstants.Name, userId.Value, error.HttpErrorCode);
            return error;
        }

        if (UserConstants.BlockedUserStatuses.Contains(user.Status))
            return new UserIsBannedError();

        if (UserConstants.ByRevisionUserStatuses.Contains(user.Status))
            return new UserIsOnRevisionError();

        stopwatch.Stop();
        
        var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        logger.LogInformation("[{ClassName}] on [{ServiceName}] User profile retrieved successfully for user ID {UserId} in {ElapsedMilliseconds} ms", nameof(GetUserProfileUseCase), ServiceNameConstants.Name, userId.Value, elapsedMilliseconds);

        return user;
    }
}
