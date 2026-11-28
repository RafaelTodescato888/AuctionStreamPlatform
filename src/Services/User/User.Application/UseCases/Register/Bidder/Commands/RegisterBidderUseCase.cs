using CrossCutting.Errors.Base;
using CrossCutting.Extensions;
using EntityFramework.Exceptions.Common;
using Microsoft.Extensions.Logging;
using OneOf;
using System.Diagnostics;
using User.Application.UseCases.Register.Seller.Commands;
using User.Domain.Constants.Service;
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
    IRegisterBidderProducer registerBidderProducer,
    ILogger<RegisterBidderUseCase> logger
) : IRegisterBidderUseCase
{
    public async Task<OneOf<bool, BaseError>> RegisterAsync(RequestRegisterUserDTO request, CancellationToken cancellationToken = default)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        logger.LogInformation("Executing [{ClassName}] on [{ServiceName}] Registering bidder", nameof(RegisterSellerUseCase), ServiceNameConstants.Name);
        var validationError = await ValidateAsync(request, cancellationToken);
        if (validationError != null)
        {
            logger.LogError("Executed [{ClassName}] on [{ServiceName}] Validation failed: {Error}", nameof(RegisterSellerUseCase), ServiceNameConstants.Name, validationError.GetType().Name);
            return validationError;
        }

        var hashedPassword = passwordHashService.GenerateHash(request.Password);
        request.SetHashedPassword(hashedPassword);

        using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var userId = await unitOfWork.UsersProfileRepository.CreateAsync(request, cancellationToken: cancellationToken);
            if (!userId.HasValue)
            {
                var error = new DatabaseError();
                logger.LogError("Executed [{ClassName}] on [{ServiceName}] Failed to create user profile, ErrorCode {ErrorCode}", nameof(RegisterSellerUseCase), ServiceNameConstants.Name, error.HttpErrorCode);
                return new DatabaseError();
            }

            var verificationResult = await generateUserVerificationUseCase.GenerateUserVerificationAsync(userId.Value, cancellationToken);
            if (verificationResult.IsError())
            {
                logger.LogError("Executed [{ClassName}] on [{ServiceName}] Failed to generate user code verification, ErrorCode {ErrorCode}", nameof(RegisterSellerUseCase), ServiceNameConstants.Name, verificationResult.GetError().HttpErrorCode);
                return verificationResult.GetError();
            }

            var producerResult = await registerBidderProducer.ProduceAsync(verificationResult.GetValue(), request.Email, cancellationToken);
            if (producerResult.IsError())
            {
                logger.LogError("Executed [{ClassName}] on [{ServiceName}] Failed to produce bidder creation message, ErrorCode {ErrorCode}", nameof(RegisterSellerUseCase), ServiceNameConstants.Name, producerResult.GetError().HttpErrorCode);

                return producerResult.GetError();
            }

            await transaction.CommitAsync(cancellationToken);

            stopwatch.Stop();

            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            logger.LogInformation("Executed [{ClassName}] on [{ServiceName}] Created bidder successfully for user ID {UserId} in {ElapsedMilliseconds} ms", nameof(RegisterSellerUseCase), ServiceNameConstants.Name, userId.Value, elapsedMilliseconds);

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
