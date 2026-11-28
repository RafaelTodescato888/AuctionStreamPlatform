using CrossCutting.Errors.Base;
using CrossCutting.Extensions;
using EntityFramework.Exceptions.Common;
using Microsoft.Extensions.Logging;
using OneOf;
using System.Diagnostics;
using User.Domain.Constants.Service;
using User.Domain.Dto.Register.Request;
using User.Domain.Enums;
using User.Domain.Errors.Common;
using User.Domain.Errors.Register;
using User.Domain.Helpers;
using User.Domain.Interfaces.Kafka.Producers.Register;
using User.Domain.Interfaces.Services.Authentication.Register;
using User.Domain.Interfaces.UoW;
using User.Domain.Interfaces.UseCase.Register.Seller.Commands;

namespace User.Application.UseCases.Register.Seller.Commands;

internal sealed class RegisterSellerUseCase(
    IUnitOfWork unitOfWork,
    IPasswordHashService passwordHashService,
    IRegisterSellerProducer registerSellerProducer,
    ILogger<RegisterSellerUseCase> logger
) : IRegisterSellerUseCase
{
    public async Task<OneOf<bool, BaseError>> RegisterAsync(RequestRegisterUserDTO request, CancellationToken cancellationToken = default)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        logger.LogInformation("Executing [{ClassName}] on [{ServiceName}] Registering seller", nameof(RegisterSellerUseCase), ServiceNameConstants.Name);
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
            var userId = await unitOfWork.UsersProfileRepository.CreateAsync(request, ERole.SELLER, cancellationToken);
            if (!userId.HasValue)
            {
                var error = new DatabaseError();
                logger.LogError("Executed [{ClassName}] on [{ServiceName}] Failed to create user profile, ErrorCode {ErrorCode}", nameof(RegisterSellerUseCase), ServiceNameConstants.Name, error.HttpErrorCode);
                return error;
            }

            var sellerApplicationId = await unitOfWork.SellersApplicationRepository.CreateAsync(userId.Value, cancellationToken);
            if (!sellerApplicationId.HasValue)
            {
                var error = new DatabaseError();
                logger.LogError("Executed [{ClassName}] on [{ServiceName}] Failed to create seller application, ErrorCode {ErrorCode}", nameof(RegisterSellerUseCase), ServiceNameConstants.Name, error.HttpErrorCode);
                return error;
            }

            var producerRequest = new RequestSellerCreationProducerDTO(
                userId.Value,
                sellerApplicationId.Value,
                DateTime.UtcNow,
                request.Name,
                request.Email
            );

            var producerResult = await registerSellerProducer.ProduceAsync(producerRequest, cancellationToken);
            if (producerResult.IsError())
            {
                logger.LogError("Executed [{ClassName}] on [{ServiceName}] Failed to produce seller creation message, ErrorCode {ErrorCode}", nameof(RegisterSellerUseCase), ServiceNameConstants.Name, producerResult.GetError().HttpErrorCode);
                return producerResult.GetError();
            }

            await transaction.CommitAsync(cancellationToken);

            stopwatch.Stop();

            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            logger.LogInformation("Executed [{ClassName}] on [{ServiceName}] Created seller successfully for user ID {UserId} in {ElapsedMilliseconds} ms", nameof(RegisterSellerUseCase), ServiceNameConstants.Name, userId.Value, elapsedMilliseconds);

            return true;
        }
        catch (UniqueConstraintException)
        {
            return new UserAlreadyRegisteredError();
        }
    }

    private async Task<BaseError?> ValidateAsync(RequestRegisterUserDTO request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return new NameIsRequiredError();

        if (string.IsNullOrWhiteSpace(request.Email))
            return new EmailIsRequiredError();

        if (request.BirthDate == default)
            return new BirthDateIsRequiredError();

        if (string.IsNullOrWhiteSpace(request.Document))
            return new DocumentIsRequiredError();

        if (!request.Document.IsValidCnpj())
            return new DocumentIsInvalidError();

        if (string.IsNullOrWhiteSpace(request.Password))
            return new PasswordIsRequiredError();

        if (await unitOfWork.UserRepository.ExistsByDocumentOrEmailAsync(request.Document, request.Email, cancellationToken))
            return new UserAlreadyRegisteredError();

        return null;
    }
}
