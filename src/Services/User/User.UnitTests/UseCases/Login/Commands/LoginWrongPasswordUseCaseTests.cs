using CrossCutting.Errors.Base;
using Microsoft.Extensions.Options;
using Moq;
using User.Application.UseCases.Login.Commands;
using User.Domain.Constants.Configuration;
using User.Domain.Errors.Login;
using User.Domain.Interfaces.Repositories.User;
using User.Domain.Interfaces.UoW;

namespace User.UnitTests.UseCases.Login.Commands;

public class LoginWrongPasswordUseCaseTests
{
    private const sbyte MaxFailedLoginAttempts = 3;

    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUsersRepository> _userRepositoryMock = new();

    private readonly LoginWrongPasswordUseCase _useCase;

    public LoginWrongPasswordUseCaseTests()
    {
        _unitOfWorkMock
            .SetupGet(x => x.UserRepository)
            .Returns(_userRepositoryMock.Object);

        var options = Options.Create(new LoginAttemptsConfig
        {
            MaxFailedLoginAttempts = MaxFailedLoginAttempts
        });

        _useCase = new LoginWrongPasswordUseCase(
            options,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task IncrementFailedLoginAttemptsOrBlockUserAsync_Should_Return_IncorrectCredentialsError_When_Attempts_Are_Below_Limit()
    {
        var userId = Guid.NewGuid();
        const sbyte failedLoginAttempts = 1;
        var cancellationToken = new CancellationTokenSource().Token;

        _userRepositoryMock
            .Setup(x => x.IncrementFailedLoginAttemptsAsync(userId, cancellationToken))
            .ReturnsAsync(true);

        var result = await _useCase.IncrementFailedLoginAttemptsOrBlockUserAsync(
            userId,
            failedLoginAttempts,
            cancellationToken);

        Assert.IsType<UserOrPasswordIsIncorrectError>(result);

        _userRepositoryMock.Verify(
            x => x.BlockUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _userRepositoryMock.Verify(
            x => x.IncrementFailedLoginAttemptsAsync(userId, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task IncrementFailedLoginAttemptsOrBlockUserAsync_Should_Return_DatabaseError_When_Incrementing_Attempts_Fails()
    {
        var userId = Guid.NewGuid();
        const sbyte failedLoginAttempts = 1;

        _userRepositoryMock
            .Setup(x => x.IncrementFailedLoginAttemptsAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _useCase.IncrementFailedLoginAttemptsOrBlockUserAsync(
            userId,
            failedLoginAttempts);

        Assert.IsType<DatabaseError>(result);

        _userRepositoryMock.Verify(
            x => x.IncrementFailedLoginAttemptsAsync(
                userId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IncrementFailedLoginAttemptsOrBlockUserAsync_Should_Return_DatabaseError_When_Blocking_User_Fails()
    {
        var userId = Guid.NewGuid();
        const sbyte failedLoginAttempts = MaxFailedLoginAttempts - 1;

        _userRepositoryMock
            .Setup(x => x.BlockUserAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _useCase.IncrementFailedLoginAttemptsOrBlockUserAsync(
            userId,
            failedLoginAttempts);

        Assert.IsType<DatabaseError>(result);

        _userRepositoryMock.Verify(
            x => x.BlockUserAsync(
                userId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _userRepositoryMock.Verify(
            x => x.IncrementFailedLoginAttemptsAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task IncrementFailedLoginAttemptsOrBlockUserAsync_Should_Return_DatabaseError_When_Incrementing_After_Blocking_Fails()
    {
        var userId = Guid.NewGuid();
        const sbyte failedLoginAttempts = MaxFailedLoginAttempts - 1;

        _userRepositoryMock
            .Setup(x => x.BlockUserAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _userRepositoryMock
            .Setup(x => x.IncrementFailedLoginAttemptsAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _useCase.IncrementFailedLoginAttemptsOrBlockUserAsync(
            userId,
            failedLoginAttempts);

        Assert.IsType<DatabaseError>(result);

        _userRepositoryMock.Verify(
            x => x.BlockUserAsync(
                userId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _userRepositoryMock.Verify(
            x => x.IncrementFailedLoginAttemptsAsync(
                userId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IncrementFailedLoginAttemptsOrBlockUserAsync_Should_Return_IncorrectCredentialsError_When_Blocking_And_Incrementing_Succeed()
    {
        var userId = Guid.NewGuid();
        const sbyte failedLoginAttempts = MaxFailedLoginAttempts - 1;
        var cancellationToken = new CancellationTokenSource().Token;

        _userRepositoryMock
            .Setup(x => x.BlockUserAsync(userId, cancellationToken))
            .ReturnsAsync(true);

        _userRepositoryMock
            .Setup(x => x.IncrementFailedLoginAttemptsAsync(userId, cancellationToken))
            .ReturnsAsync(true);

        var result = await _useCase.IncrementFailedLoginAttemptsOrBlockUserAsync(
            userId,
            failedLoginAttempts,
            cancellationToken);

        Assert.IsType<UserOrPasswordIsIncorrectError>(result);

        _userRepositoryMock.Verify(
            x => x.BlockUserAsync(userId, cancellationToken),
            Times.Once);

        _userRepositoryMock.Verify(
            x => x.IncrementFailedLoginAttemptsAsync(userId, cancellationToken),
            Times.Once);
    }
}
