using CrossCutting.Errors.Base;
using OneOf;
using User.Domain.Dto.Profile;

namespace User.Domain.Interfaces.UseCase.Profile.Queries
{
    public interface IGetUserProfileUseCase
    {
        Task<OneOf<UserProfileResponseDTO, BaseError>> GetAsync(CancellationToken cancellationToken = default);
    }
}
