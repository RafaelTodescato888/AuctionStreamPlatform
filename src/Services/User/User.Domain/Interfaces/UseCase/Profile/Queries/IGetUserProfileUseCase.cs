using CrossCutting.Errors.Base;
using OneOf;
using User.Domain.Dto.Profile.Response;

namespace User.Domain.Interfaces.UseCase.Profile.Queries;

public interface IGetUserProfileUseCase
{
    Task<OneOf<ResponseUserProfileDTO, BaseError>> GetAsync(CancellationToken cancellationToken = default);
}
