using CrossCutting.Errors.Base;
using Moq;
using OneOf;
using User.Application.UseCases.Register.Verification.Commands;
using User.Domain.Dto.Register.Request;
using User.Domain.Enums;
using User.Domain.Errors.Register;
using User.Domain.Interfaces.Repositories.UserVerification;
using User.Domain.Interfaces.Services.Authentication.Register;
using User.Domain.Interfaces.UoW;

namespace User.UnitTests.UseCases.Register.Verification.Commands;

public class GenerateUserVerificationUseCaseTests
{
    private readonly Mock<IGenerateNumberCodeService> _generateNumberCodeServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUsersVerificationRepository> _usersVerificationRepositoryMock = new();

    private readonly GenerateUserVerificationUseCase _useCase;

    public GenerateUserVerificationUseCaseTests()
    {
        _unitOfWorkMock
            .SetupGet(x => x.UsersVerificationRepository)
            .Returns(_usersVerificationRepositoryMock.Object);

        _useCase = new GenerateUserVerificationUseCase(
            _generateNumberCodeServiceMock.Object,
            _unitOfWorkMock.Object
        );
    }

    // -------------------------------------------------------
    // Successful generation
    // -------------------------------------------------------

    [Fact]
    public async Task GenerateUserVerificationAsync_Should_Return_Response_When_GenerationSucceeds()
    {
        var userId = Guid.NewGuid();
        var code = "123456";

        _generateNumberCodeServiceMock
            .Setup(x => x.GenerateNumberCodeAsync(It.IsAny<int>()))
            .Returns((OneOf<string, BaseError>)code);

        _usersVerificationRepositoryMock
            .Setup(x => x.UpsertAsync(
                It.IsAny<RequestGenerateUserVerificationDTO>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _useCase.GenerateUserVerificationAsync(userId);

        Assert.True(result.IsT0);

        var response = result.AsT0;

        Assert.Equal(userId, response.UserId);
        Assert.Equal(EVerificationType.EMAIL_CONFIRMATION, response.VerificationType);
        Assert.Equal(code, response.Code);

        Assert.InRange(
            response.ExpiresAt,
            DateTime.UtcNow.AddMinutes(4).AddSeconds(50),
            DateTime.UtcNow.AddMinutes(5).AddSeconds(10)
        );

        _generateNumberCodeServiceMock.Verify(
            x => x.GenerateNumberCodeAsync(It.IsAny<int>()),
            Times.Once);

        _usersVerificationRepositoryMock.Verify(
            x => x.UpsertAsync(
                It.Is<RequestGenerateUserVerificationDTO>(request =>
                    request.UserId == userId &&
                    request.Type == EVerificationType.EMAIL_CONFIRMATION &&
                    request.HashCode == code),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // -------------------------------------------------------
    // Code generation failures
    // -------------------------------------------------------

    [Fact]
    public async Task GenerateUserVerificationAsync_Should_Return_Error_When_CodeGenerationFails()
    {
        var userId = Guid.NewGuid();

        var expectedError = new CodeSizeShouldntBeNegative();

        _generateNumberCodeServiceMock
            .Setup(x => x.GenerateNumberCodeAsync(It.IsAny<int>()))
            .Returns((OneOf<string, BaseError>)expectedError);

        var result = await _useCase.GenerateUserVerificationAsync(userId);

        Assert.True(result.IsT1);
        Assert.Same(expectedError, result.AsT1);

        _usersVerificationRepositoryMock.Verify(
            x => x.UpsertAsync(
                It.IsAny<RequestGenerateUserVerificationDTO>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // -------------------------------------------------------
    // Database failures
    // -------------------------------------------------------

    [Fact]
    public async Task GenerateUserVerificationAsync_Should_Return_DatabaseError_When_UpsertFails()
    {
        var userId = Guid.NewGuid();
        var code = "123456";

        _generateNumberCodeServiceMock
            .Setup(x => x.GenerateNumberCodeAsync(It.IsAny<int>()))
            .Returns((OneOf<string, BaseError>)code);

        _usersVerificationRepositoryMock
            .Setup(x => x.UpsertAsync(
                It.IsAny<RequestGenerateUserVerificationDTO>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _useCase.GenerateUserVerificationAsync(userId);

        Assert.True(result.IsT1);
        Assert.IsType<DatabaseError>(result.AsT1);

        _usersVerificationRepositoryMock.Verify(
            x => x.UpsertAsync(
                It.Is<RequestGenerateUserVerificationDTO>(request =>
                    request.UserId == userId &&
                    request.Type == EVerificationType.EMAIL_CONFIRMATION &&
                    request.HashCode == code),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // -------------------------------------------------------
    // Cancellation
    // -------------------------------------------------------

    [Fact]
    public async Task GenerateUserVerificationAsync_Should_Propagate_CancellationToken_To_Repository()
    {
        var userId = Guid.NewGuid();
        var code = "123456";
        var cancellationToken = new CancellationTokenSource().Token;

        _generateNumberCodeServiceMock
            .Setup(x => x.GenerateNumberCodeAsync(It.IsAny<int>()))
            .Returns((OneOf<string, BaseError>)code);

        _usersVerificationRepositoryMock
            .Setup(x => x.UpsertAsync(
                It.IsAny<RequestGenerateUserVerificationDTO>(),
                cancellationToken))
            .ReturnsAsync(true);

        await _useCase.GenerateUserVerificationAsync(userId, cancellationToken);

        _usersVerificationRepositoryMock.Verify(
            x => x.UpsertAsync(
                It.IsAny<RequestGenerateUserVerificationDTO>(),
                cancellationToken),
            Times.Once);
    }

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

}