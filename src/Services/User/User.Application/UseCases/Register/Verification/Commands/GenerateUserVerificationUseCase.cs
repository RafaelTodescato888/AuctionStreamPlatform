using CrossCutting.Errors.Base;
using CrossCutting.Extensions;
using OneOf;
using User.Domain.Dto.Register.Request;
using User.Domain.Dto.Register.Response;
using User.Domain.Enums;
using User.Domain.Interfaces.Services.Authentication.Register;
using User.Domain.Interfaces.UoW;
using User.Domain.Interfaces.UseCase.Register.Verification.Commands;

namespace User.Application.UseCases.Register.Verification.Commands;

public class GenerateUserVerificationUseCase(
    IGenerateNumberCodeService generateNumberCodeService,
    IUnitOfWork unitOfWork
) : IGenerateUserVerificationUseCase
{
    public async Task<OneOf<ResponseGenerateUserVerificationDTO, BaseError>> GenerateUserVerificationAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var codeResponse = generateNumberCodeService.GenerateNumberCodeAsync();
        if (codeResponse.IsError())
            return codeResponse.GetError();

        var code = codeResponse.GetValue();

        var usersVerificationRequest = new RequestGenerateUserVerificationDTO(
            userId,
            EVerificationType.EMAIL_CONFIRMATION,
            code,
            DateTime.UtcNow.AddMinutes(5)
        );

        if (!await unitOfWork.UsersVerificationRepository.UpsertAsync(usersVerificationRequest, cancellationToken))
            return new DatabaseError();

        return CreateResponseGenerateUserVerificationDTO(usersVerificationRequest);
    }

    private static ResponseGenerateUserVerificationDTO CreateResponseGenerateUserVerificationDTO(
        RequestGenerateUserVerificationDTO request)
    {
        return new ResponseGenerateUserVerificationDTO(
            request.UserId,
            request.Type,
            request.HashCode,
            request.ExpiresAt
        );
    }
}
