using CrossCutting.Errors.Base;
using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OneOf;
using User.Application.UseCases.Register.Seller.Commands;
using User.Domain.Dto.Register.Request;
using User.Domain.Dto.Register.Response;
using User.Domain.Enums;
using User.Domain.Errors.Common;
using User.Domain.Errors.Register;
using User.Domain.Interfaces.Kafka.Producers.Register;
using User.Domain.Interfaces.Repositories.SellerApplication;
using User.Domain.Interfaces.Repositories.User;
using User.Domain.Interfaces.Repositories.UserProfile;
using User.Domain.Interfaces.Services.Authentication.Register;
using User.Domain.Interfaces.UoW;

namespace User.UnitTests.UseCases.Register.Seller.Commands;

public class RegisterSellerUseCaseTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IPasswordHashService> _passwordHashServiceMock = new();

    private readonly Mock<IUsersRepository> _userRepositoryMock = new();
    private readonly Mock<IUsersProfileRepository> _usersProfileRepositoryMock = new();
    private readonly Mock<ISellersApplicationRepository> _sellersApplicationRepositoryMock = new();
    private readonly Mock<IDbContextTransaction> _transactionMock = new();
    private readonly Mock<IRegisterSellerProducer> _registerSellerProducerMock = new();
    private readonly ILogger<RegisterSellerUseCase> _logger = NullLogger<RegisterSellerUseCase>.Instance;

    private readonly RegisterSellerUseCase _useCase;

    public RegisterSellerUseCaseTests()
    {
        _unitOfWorkMock
            .SetupGet(x => x.UserRepository)
            .Returns(_userRepositoryMock.Object);

        _unitOfWorkMock
            .SetupGet(x => x.UsersProfileRepository)
            .Returns(_usersProfileRepositoryMock.Object);

        _unitOfWorkMock
            .SetupGet(x => x.SellersApplicationRepository)
            .Returns(_sellersApplicationRepositoryMock.Object);

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

        _registerSellerProducerMock
            .Setup(x => x.ProduceAsync(
                It.IsAny<RequestSellerCreationProducerDTO>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((OneOf<bool, BaseError>)true);

        _useCase = new RegisterSellerUseCase(
            _unitOfWorkMock.Object,
            _passwordHashServiceMock.Object,
            _registerSellerProducerMock.Object,
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
    [InlineData("11111111111111")]
    [InlineData("00000000000000")]
    [InlineData("11222333000180")]
    [InlineData("abcdefghijklm")]
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

        _passwordHashServiceMock.Verify(
            x => x.GenerateHash(It.IsAny<string>()),
            Times.Never);

        _usersProfileRepositoryMock.Verify(
            x => x.CreateAsync(
                It.IsAny<RequestRegisterUserDTO>(),
                It.IsAny<ERole>(),
                It.IsAny<CancellationToken>()),
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
        var sellerApplicationId = Guid.NewGuid();

        _usersProfileRepositoryMock
            .Setup(x => x.CreateAsync(
                request,
                ERole.SELLER,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        _sellersApplicationRepositoryMock
            .Setup(x => x.CreateAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(sellerApplicationId);

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
                ERole.SELLER,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _sellersApplicationRepositoryMock.Verify(
            x => x.CreateAsync(
                userId,
                It.IsAny<CancellationToken>()),
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
                ERole.SELLER,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        var result = await _useCase.RegisterAsync(request);

        Assert.True(result.IsT1);
        Assert.IsType<DatabaseError>(result.AsT1);

        _usersProfileRepositoryMock.Verify(
            x => x.CreateAsync(
                request,
                ERole.SELLER,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_Should_Return_UserAlreadyRegisteredError_When_UniqueConstraintIsViolated()
    {
        var request = CreateValidRequest();

        _usersProfileRepositoryMock
            .Setup(x => x.CreateAsync(
                request,
                ERole.SELLER,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new UniqueConstraintException(
                    "Unique constraint violation",
                    new Exception()));

        var result = await _useCase.RegisterAsync(request);

        Assert.True(result.IsT1);
        Assert.IsType<UserAlreadyRegisteredError>(result.AsT1);
    }

    // -------------------------------------------------------
    // Cancellation
    // -------------------------------------------------------

    [Fact]
    public async Task RegisterAsync_Should_Propagate_CancellationToken_To_Dependencies()
    {
        var request = CreateValidRequest();
        var cancellationToken = new CancellationTokenSource().Token;

        _usersProfileRepositoryMock
            .Setup(x => x.CreateAsync(
                request,
                ERole.SELLER,
                cancellationToken))
            .ReturnsAsync(Guid.NewGuid());

        _sellersApplicationRepositoryMock
            .Setup(x => x.CreateAsync(
                It.IsAny<Guid>(),
                cancellationToken))
            .ReturnsAsync(Guid.NewGuid());

        await _useCase.RegisterAsync(request, cancellationToken);

        _userRepositoryMock.Verify(
            x => x.ExistsByDocumentOrEmailAsync(
                request.Document,
                It.IsAny<string>(),
                cancellationToken),
            Times.Once);

        _usersProfileRepositoryMock.Verify(
            x => x.CreateAsync(
                request,
                ERole.SELLER,
                cancellationToken),
            Times.Once);
    }

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

    private void VerifyRegistrationWasNotStarted()
    {
        _passwordHashServiceMock.Verify(
            x => x.GenerateHash(It.IsAny<string>()),
            Times.Never);

        _usersProfileRepositoryMock.Verify(
            x => x.CreateAsync(
                It.IsAny<RequestRegisterUserDTO>(),
                It.IsAny<ERole>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static RequestRegisterUserDTO CreateValidRequest(
        string name = "Test Seller",
        string email = "seller@email.com",
        string? bio = "Seller bio",
        DateOnly? birthDate = null,
        string document = "11222333000181",
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
}