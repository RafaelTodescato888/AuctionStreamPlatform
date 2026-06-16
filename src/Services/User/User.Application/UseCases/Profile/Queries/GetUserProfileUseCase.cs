using CrossCutting.Errors.Base;
using Microsoft.AspNetCore.Http;
using OneOf;
using User.Domain.Dto.Profile;
using User.Domain.Errors.Common;
using User.Domain.Errors.Common.Authentication;
using User.Domain.Helpers;
using User.Domain.Interfaces.UoW;
using User.Domain.Interfaces.UseCase.Profile.Queries;

namespace User.Application.UseCases.Profile.Queries
{
    internal sealed class GetUserProfileUseCase(
        IHttpContextAccessor httpContextAccessor,
        IUnitOfWork unitOfWork
    ) : IGetUserProfileUseCase
    {
        public async Task<OneOf<UserProfileResponseDTO, BaseError>> GetAsync(CancellationToken cancellationToken = default)
        {
            var userId = SessionHelper.GetUserId(httpContextAccessor.HttpContext);
            if (!userId.HasValue)
                return new UserIsNotAuthorizedError();

            var user = await unitOfWork.UsersProfileRepository.GetAsync(userId.Value, cancellationToken);
            if (user == null)
                return new UserNotFoundError();

            return user;
        }
    }
}
