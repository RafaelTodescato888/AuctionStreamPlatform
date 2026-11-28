using CrossCutting.Errors.Base;
using OneOf;
using User.Domain.Dto.Register.Response;

namespace User.Domain.Interfaces.UseCase.Register.Verification.Commands;

public interface IGenerateUserVerificationUseCase
{
    Task<OneOf<ResponseGenerateUserVerificationDTO, BaseError>> GenerateUserVerificationAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
