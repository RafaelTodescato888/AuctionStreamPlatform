using CrossCutting.Errors.Base;
using CrossCutting.Extensions;
using EntityFramework.Exceptions.Common;
using OneOf;
using User.Domain.Dto.Register.Request;
using User.Domain.Errors.Common;
using User.Domain.Errors.Register;
using User.Domain.Helpers;
using User.Domain.Interfaces.Kafka.Producers.Register;
using User.Domain.Interfaces.Services.Authentication.Register;
using User.Domain.Interfaces.UoW;
using User.Domain.Interfaces.UseCase.Register.Bidder.Commands;
using User.Domain.Interfaces.UseCase.Register.Verification.Commands;

namespace User.Application.UseCases.Register.Bidder.Commands;

internal sealed class RegisterBidderUseCase(
    IUnitOfWork unitOfWork,
    IPasswordHashService passwordHashService,
    IGenerateUserVerificationUseCase generateUserVerificationUseCase,
    IRegisterBidderProducer registerBidderProducer
) : IRegisterBidderUseCase
{
    public async Task<OneOf<bool, BaseError>> RegisterAsync(RequestRegisterUserDTO request, CancellationToken cancellationToken = default)
    {
        var error = await ValidateAsync(request, cancellationToken);
        if (error != null)
            return error;

        var hashedPassword = passwordHashService.GenerateHash(request.Password);
        request.SetHashedPassword(hashedPassword);

        using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var userId = await unitOfWork.UsersProfileRepository.CreateAsync(request, cancellationToken: cancellationToken);
            if (!userId.HasValue)
                return new DatabaseError();

            var verificationResult = await generateUserVerificationUseCase.GenerateUserVerificationAsync(userId.Value, cancellationToken);
            if (verificationResult.IsError())
                return verificationResult.GetError();

            var producerResult = await registerBidderProducer.ProduceAsync(verificationResult.GetValue(), request.Email, cancellationToken);
            if (producerResult.IsError())
                return producerResult.GetError();

            await transaction.CommitAsync(cancellationToken);

            return true;
        }
        catch (UniqueConstraintException)
        {
            return new UserAlreadyRegisteredError();
        }
    }

    private async Task<BaseError?> ValidateAsync(RequestRegisterUserDTO request, CancellationToken cancellationToken = default)
    {
        if(string.IsNullOrWhiteSpace(request.Name))
            return new NameIsRequiredError();

        if(string.IsNullOrWhiteSpace(request.Email))
            return new EmailIsRequiredError();

        if(request.BirthDate == default)
            return new BirthDateIsRequiredError();

        if(string.IsNullOrWhiteSpace(request.Document))
            return new DocumentIsRequiredError();

        if(!request.Document.IsValidCpf())
            return new DocumentIsInvalidError();

        if(string.IsNullOrWhiteSpace(request.Password))
            return new PasswordIsRequiredError();

        if(await unitOfWork.UserRepository.ExistsByDocumentOrEmailAsync(request.Document, request.Email, cancellationToken))
            return new UserAlreadyRegisteredError();

        return null;
    }
}
