using Castle.Core.Logging;
using CrossCutting.Errors.Base;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OneOf;
using User.Application.UseCases.Profile.Queries;
using User.Application.UseCases.Register.Bidder.Commands;
using User.Domain.Dto.Register.Request;
using User.Domain.Dto.Register.Response;
using User.Domain.Enums;
using User.Domain.Errors.Common;
using User.Domain.Errors.Register;
using User.Domain.Interfaces.Kafka.Producers.Register;
using User.Domain.Interfaces.Repositories.User;
using User.Domain.Interfaces.Repositories.UserProfile;
using User.Domain.Interfaces.Services.Authentication.Register;
using User.Domain.Interfaces.UoW;
using User.Domain.Interfaces.UseCase.Register.Verification.Commands;

namespace User.UnitTests.UseCases.Register.Bidder.Commands;

public class RegisterBidderUseCaseTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IPasswordHashService> _passwordHashServiceMock = new();
    private readonly Mock<IGenerateUserVerificationUseCase> _generateUserVerificationMock = new();
    private readonly Mock<IRegisterBidderProducer> _registerBidderProducerMock = new();

    private readonly Mock<IUsersRepository> _userRepositoryMock = new();
    private readonly Mock<IUsersProfileRepository> _usersProfileRepositoryMock = new();
    private readonly Mock<IDbContextTransaction> _transactionMock = new();
    private readonly ILogger<RegisterBidderUseCase> _logger = NullLogger<RegisterBidderUseCase>.Instance;

    private readonly RegisterBidderUseCase _useCase;

    public RegisterBidderUseCaseTests()
    {
        _unitOfWorkMock
            .SetupGet(x => x.UserRepository)
            .Returns(_userRepositoryMock.Object);

        _unitOfWorkMock
            .SetupGet(x => x.UsersProfileRepository)
            .Returns(_usersProfileRepositoryMock.Object);

        _unitOfWorkMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_transactionMock.Object);

        _userRepositoryMock
            .Setup(x => x.ExistsByDocumentOrEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _passwordHashServiceMock
            .Setup(x => x.GenerateHash(It.IsAny<string>()))
            .Returns("hashed-password");

        _useCase = new RegisterBidderUseCase(
            _unitOfWorkMock.Object,
            _passwordHashServiceMock.Object,
            _generateUserVerificationMock.Object,
            _registerBidderProducerMock.Object,
            _logger
        );
    }

    // -------------------------------------------------------
    // Validation
    // -------------------------------------------------------

    [Fact]
    public async Task RegisterAsync_Should_Return_NameIsRequiredError_When_NameIsEmpty()
    {
        var request = CreateValidRequest(name: "");

        var result = await _useCase.RegisterAsync(request);

        Assert.True(result.IsT1);
        Assert.IsType<NameIsRequiredError>(result.AsT1);

        VerifyRegistrationWasNotStarted();
    }

    [Fact]
    public async Task RegisterAsync_Should_Return_EmailIsRequiredError_When_EmailIsEmpty()
    {
        var request = CreateValidRequest(email: "");

        var result = await _useCase.RegisterAsync(request);

        Assert.True(result.IsT1);
        Assert.IsType<EmailIsRequiredError>(result.AsT1);

        VerifyRegistrationWasNotStarted();
    }

    [Fact]
    public async Task RegisterAsync_Should_Return_BirthDateIsRequiredError_When_BirthDateIsDefault()
    {
        var request = CreateValidRequest(useDefaultBirthDate: true);

        var result = await _useCase.RegisterAsync(request);

        Assert.True(result.IsT1);
        Assert.IsType<BirthDateIsRequiredError>(result.AsT1);

        VerifyRegistrationWasNotStarted();
    }

    [Fact]
    public async Task RegisterAsync_Should_Return_DocumentIsRequiredError_When_DocumentIsEmpty()
    {
        var request = CreateValidRequest(document: "");

        var result = await _useCase.RegisterAsync(request);

        Assert.True(result.IsT1);
        Assert.IsType<DocumentIsRequiredError>(result.AsT1);

        VerifyRegistrationWasNotStarted();
    }

    [Theory]
    [InlineData("123")]
    [InlineData("11111111111")]
    [InlineData("52998224724")]
    [InlineData("abcdefghijk")]
    public async Task RegisterAsync_Should_Return_DocumentIsInvalidError_When_DocumentIsInvalid(
        string document)
    {
        var request = CreateValidRequest(document: document);

        var result = await _useCase.RegisterAsync(request);

        Assert.True(result.IsT1);
        Assert.IsType<DocumentIsInvalidError>(result.AsT1);

        VerifyRegistrationWasNotStarted();
    }

    [Fact]
    public async Task RegisterAsync_Should_Return_PasswordIsRequiredError_When_PasswordIsEmpty()
    {
        var request = CreateValidRequest(password: "");

        var result = await _useCase.RegisterAsync(request);

        Assert.True(result.IsT1);
        Assert.IsType<PasswordIsRequiredError>(result.AsT1);

        VerifyRegistrationWasNotStarted();
    }

    [Fact]
    public async Task RegisterAsync_Should_Return_UserAlreadyRegisteredError_When_DocumentAlreadyExists()
    {
        var request = CreateValidRequest();

        _userRepositoryMock
            .Setup(x => x.ExistsByDocumentOrEmailAsync(
                request.Document,
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _useCase.RegisterAsync(request);

        Assert.True(result.IsT1);
        Assert.IsType<UserAlreadyRegisteredError>(result.AsT1);

        _unitOfWorkMock.Verify(
            x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        _passwordHashServiceMock.Verify(
            x => x.GenerateHash(It.IsAny<string>()),
            Times.Never);
    }

    // -------------------------------------------------------
    // Successful registration
    // -------------------------------------------------------

    [Fact]
    public async Task RegisterAsync_Should_Return_True_When_RegistrationSucceeds()
    {
        var request = CreateValidRequest();
        var userId = Guid.NewGuid();
        var verification = CreateVerificationResponse(userId);

        SetupSuccessfulRegistration(request, userId, verification);

        var result = await _useCase.RegisterAsync(request);

        Assert.True(result.IsT0);
        Assert.True(result.AsT0);

        Assert.Equal("hashed-password", request.HashedPassword);

        _passwordHashServiceMock.Verify(
            x => x.GenerateHash("password"),
            Times.Once);

        _usersProfileRepositoryMock.Verify(
            x => x.CreateAsync(
                request,
                ERole.BIDDER,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _generateUserVerificationMock.Verify(
            x => x.GenerateUserVerificationAsync(
                userId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _registerBidderProducerMock.Verify(
            x => x.ProduceAsync(
                verification,
                request.Email,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _transactionMock.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // -------------------------------------------------------
    // Database failures
    // -------------------------------------------------------

    [Fact]
    public async Task RegisterAsync_Should_Return_DatabaseError_When_UserCreationFails()
    {
        var request = CreateValidRequest();

        _usersProfileRepositoryMock
            .Setup(x => x.CreateAsync(
                request,
                ERole.BIDDER,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        var result = await _useCase.RegisterAsync(request);

        Assert.True(result.IsT1);
        Assert.IsType<DatabaseError>(result.AsT1);

        _generateUserVerificationMock.Verify(
            x => x.GenerateUserVerificationAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _registerBidderProducerMock.Verify(
            x => x.ProduceAsync(
                It.IsAny<ResponseGenerateUserVerificationDTO>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _transactionMock.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // -------------------------------------------------------
    // Verification failures
    // -------------------------------------------------------

    [Fact]
    public async Task RegisterAsync_Should_Return_Error_When_GeneratingVerificationFails()
    {
        var request = CreateValidRequest();
        var userId = Guid.NewGuid();

        _usersProfileRepositoryMock
            .Setup(x => x.CreateAsync(
                request,
                ERole.BIDDER,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        _generateUserVerificationMock
            .Setup(x => x.GenerateUserVerificationAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((OneOf<ResponseGenerateUserVerificationDTO, BaseError>)
                new DatabaseError());

        var result = await _useCase.RegisterAsync(request);

        Assert.True(result.IsT1);
        Assert.IsType<DatabaseError>(result.AsT1);

        _registerBidderProducerMock.Verify(
            x => x.ProduceAsync(
                It.IsAny<ResponseGenerateUserVerificationDTO>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _transactionMock.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // -------------------------------------------------------
    // Kafka producer failures
    // -------------------------------------------------------

    [Fact]
    public async Task RegisterAsync_Should_Return_Error_When_ProducingMessageFails()
    {
        var request = CreateValidRequest();
        var userId = Guid.NewGuid();
        var verification = CreateVerificationResponse(userId);

        _usersProfileRepositoryMock
            .Setup(x => x.CreateAsync(
                request,
                ERole.BIDDER,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        _generateUserVerificationMock
            .Setup(x => x.GenerateUserVerificationAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((OneOf<ResponseGenerateUserVerificationDTO, BaseError>)
                verification);

        _registerBidderProducerMock
            .Setup(x => x.ProduceAsync(
                verification,
                request.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((OneOf<bool, BaseError>)new DatabaseError());

        var result = await _useCase.RegisterAsync(request);

        Assert.True(result.IsT1);
        Assert.IsType<DatabaseError>(result.AsT1);

        _transactionMock.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // -------------------------------------------------------
    // Cancellation
    // -------------------------------------------------------

    [Fact]
    public async Task RegisterAsync_Should_Propagate_CancellationToken_To_Dependencies()
    {
        var request = CreateValidRequest();
        var cancellationToken = new CancellationTokenSource().Token;

        var userId = Guid.NewGuid();
        var verification = CreateVerificationResponse(userId);

        SetupSuccessfulRegistration(request, userId, verification);

        await _useCase.RegisterAsync(request, cancellationToken);

        _userRepositoryMock.Verify(
            x => x.ExistsByDocumentOrEmailAsync(
                request.Document,
                It.IsAny<string>(),
                cancellationToken),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.BeginTransactionAsync(cancellationToken),
            Times.Once);

        _usersProfileRepositoryMock.Verify(
            x => x.CreateAsync(
                request,
                ERole.BIDDER,
                cancellationToken),
            Times.Once);

        _generateUserVerificationMock.Verify(
            x => x.GenerateUserVerificationAsync(
                userId,
                cancellationToken),
            Times.Once);

        _registerBidderProducerMock.Verify(
            x => x.ProduceAsync(
                verification,
                request.Email,
                cancellationToken),
            Times.Once);

        _transactionMock.Verify(
            x => x.CommitAsync(cancellationToken),
            Times.Once);
    }

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

    private void SetupSuccessfulRegistration(
        RequestRegisterUserDTO request,
        Guid userId,
        ResponseGenerateUserVerificationDTO verification)
    {
        _usersProfileRepositoryMock
            .Setup(x => x.CreateAsync(
                request,
                ERole.BIDDER,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        _generateUserVerificationMock
            .Setup(x => x.GenerateUserVerificationAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((OneOf<ResponseGenerateUserVerificationDTO, BaseError>)
                verification);

        _registerBidderProducerMock
            .Setup(x => x.ProduceAsync(
                verification,
                request.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((OneOf<bool, BaseError>)true);
    }

    private void VerifyRegistrationWasNotStarted()
    {
        _passwordHashServiceMock.Verify(
            x => x.GenerateHash(It.IsAny<string>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        _usersProfileRepositoryMock.Verify(
            x => x.CreateAsync(
                It.IsAny<RequestRegisterUserDTO>(),
                It.IsAny<ERole>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static RequestRegisterUserDTO CreateValidRequest(
        string name = "Test User",
        string email = "test@email.com",
        string? bio = "Test bio",
        DateOnly? birthDate = null,
        string document = "52998224725",
        string password = "password",
        bool useDefaultBirthDate = false)
    {
        return new RequestRegisterUserDTO(
            name,
            email,
            bio,
            useDefaultBirthDate
                ? default
                : birthDate ?? new DateOnly(2000, 1, 1),
            document,
            password
        );
    }

    private static ResponseGenerateUserVerificationDTO CreateVerificationResponse(
        Guid userId)
    {
        return new ResponseGenerateUserVerificationDTO(
            userId,
            EVerificationType.EMAIL_CONFIRMATION,
            "123456",
            DateTime.UtcNow.AddMinutes(5)
        );
    }
}