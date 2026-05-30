using CrossCutting.Errors.Base;
using Microsoft.AspNetCore.Http;
using OneOf;
using User.Domain.Dto.Update.Request;
using User.Domain.Errors.Common;
using User.Domain.Errors.Common.Authentication;
using User.Domain.Helpers;
using User.Domain.Interfaces.UoW;
using User.Domain.Interfaces.UseCase.Update.Commands;

namespace User.Application.UseCases.Update.Commands;

internal sealed class UpdateProfileUserUseCase(
    IHttpContextAccessor httpContextAccessor,
    IUnitOfWork unitOfWork
) : IUpdateProfileUserUseCase
{
    public async Task<OneOf<bool, BaseError>> UpdateAsync(RequestUpdateProfileUserDTO request, CancellationToken cancellationToken)
    {
        var userId = SessionHelper.GetUserId(httpContextAccessor.HttpContext);
        if (!userId.HasValue)
            return new UserIsNotAuthorizedError();

        var errorValidation = await ValidateAsync(request, userId.Value, cancellationToken);

        if (errorValidation != null)
            return errorValidation;

        var success = await unitOfWork.UsersProfileRepository.UpdateAsync(request, userId.Value, cancellationToken);
        if (!success)
            return new DatabaseError();

        return true;
    }

    private async Task<BaseError?> ValidateAsync(RequestUpdateProfileUserDTO request, Guid userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return new NameIsRequiredError();

        if (!await unitOfWork.UsersProfileRepository.ExistsByUserIdAsync(userId, cancellationToken))
            return new UserNotFoundError();

        return null;
    }
}
