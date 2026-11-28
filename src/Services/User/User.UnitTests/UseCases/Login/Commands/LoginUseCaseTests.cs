using CrossCutting.Errors.Base;
using Moq;
using User.Application.UseCases.Login.Commands;
using User.Domain.Dto.Common;
using User.Domain.Dto.Login.Request;
using User.Domain.Dto.Register.Projections;
using User.Domain.Enums;
using User.Domain.Errors.Login;
using User.Domain.Interfaces.Repositories.User;
using User.Domain.Interfaces.Repositories.UserSession;
using User.Domain.Interfaces.Services.Authentication.Login;
using User.Domain.Interfaces.Services.Authentication.Register;
using User.Domain.Interfaces.UoW;

namespace User.UnitTests.UseCases.Login.Commands;

public class LoginUseCaseTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPasswordHashService> _passwordHashServiceMock;
    private readonly Mock<IGenerateTokenService> _generateTokenServiceMock;

    private readonly LoginUseCase _useCase;

    public LoginUseCaseTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _passwordHashServiceMock = new Mock<IPasswordHashService>();
        _generateTokenServiceMock = new Mock<IGenerateTokenService>();

        _useCase = new LoginUseCase(
            _unitOfWorkMock.Object,
            _passwordHashServiceMock.Object,
            _generateTokenServiceMock.Object
        );
    }

    [Fact]
    public async Task LoginAsync_Should_Return_Valid_Tokens_When_Credentials_Are_Correct()
    {
        var userId = Guid.NewGuid();
        var email = "john@email.com";
        var password = "password";
        var hashedPassword = "hashed-password";

        var accessToken = "access-token";

        var refreshToken = new RefreshTokenDTO(
            "refresh-token",
            DateTime.UtcNow.AddDays(7)
        );

        var content = new RequestUserLoginDTO(email, password);

        var userInformations = new UserCredentialsProjection(new RequestGenerateTokenDTO(
            userId,
            "John Doe",
            ERole.BIDDER,
            EUserStatus.ACTIVE
        ), hashedPassword);

        var userRepositoryMock = new Mock<
            User.Domain.Interfaces.Repositories.User.IUsersRepository
        >();

        var sessionRepositoryMock = new Mock<
            User.Domain.Interfaces.Repositories.UserSession.IUsersSessionRepository
        >();

        _unitOfWorkMock
            .SetupGet(u => u.UserRepository)
            .Returns(userRepositoryMock.Object);

        _unitOfWorkMock
            .SetupGet(u => u.UsersSessionRepository)
            .Returns(sessionRepositoryMock.Object);

        _passwordHashServiceMock
            .Setup(p => p.GenerateHash(password))
            .Returns(hashedPassword);

        userRepositoryMock
            .Setup(r => r.GetUserByCredentialsAsync(
                It.IsAny<RequestUserLoginDTO>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(userInformations);

        _generateTokenServiceMock
            .Setup(g => g.GenerateRefreshToken())
            .Returns(refreshToken);

        sessionRepositoryMock
            .Setup(r => r.UpsertAsync(
                refreshToken,
                userId,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(true);

        _generateTokenServiceMock
            .Setup(g => g.GenerateToken(userInformations.User))
            .Returns(accessToken);

        var result = await _useCase.LoginAsync(content);

        Assert.True(result.IsT0);

        var response = result.AsT0;

        Assert.Equal(accessToken, response.Token);
        Assert.Equal(refreshToken.RefreshToken, response.RefreshToken);
        Assert.Equal(hashedPassword, content.Password);

        _passwordHashServiceMock.Verify(
            p => p.GenerateHash(password),
            Times.Once
        );

        userRepositoryMock.Verify(
            r => r.GetUserByCredentialsAsync(
                content,
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        sessionRepositoryMock.Verify(
            r => r.UpsertAsync(
                refreshToken,
                userId,
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        _generateTokenServiceMock.Verify(
            g => g.GenerateToken(userInformations.User),
            Times.Once
        );
    }

    [Fact]
    public async Task LoginAsync_Should_Return_UserOrPasswordIsIncorrectError_When_User_Does_Not_Exist()
    {
        var content = new RequestUserLoginDTO(
            "john@email.com",
            "password"
        );

        var userRepositoryMock = new Mock<
            User.Domain.Interfaces.Repositories.User.IUsersRepository
        >();

        _unitOfWorkMock
            .SetupGet(u => u.UserRepository)
            .Returns(userRepositoryMock.Object);

        _passwordHashServiceMock
            .Setup(p => p.GenerateHash(content.Password))
            .Returns("hashed-password");

        userRepositoryMock
            .Setup(r => r.GetUserByCredentialsAsync(
                content,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((UserCredentialsProjection?)null);

        var result = await _useCase.LoginAsync(content);

        Assert.True(result.IsT1);
        Assert.IsType<UserOrPasswordIsIncorrectError>(result.AsT1);

        _generateTokenServiceMock.Verify(
            g => g.GenerateRefreshToken(),
            Times.Never
        );
    }

    [Fact]
    public async Task LoginAsync_Should_Return_UserIsBannedError_When_User_Is_Banned()
    {
        var userId = Guid.NewGuid();

        var content = new RequestUserLoginDTO(
            "john@email.com",
            "password"
        );

        var userInformations = new UserCredentialsProjection(
            new RequestGenerateTokenDTO(
                userId,
                "John Doe",
                ERole.BIDDER,
                EUserStatus.BANNED
            ),
            "hashed-password"
        );

        var userRepositoryMock = new Mock<
            User.Domain.Interfaces.Repositories.User.IUsersRepository
        >();

        _unitOfWorkMock
            .SetupGet(u => u.UserRepository)
            .Returns(userRepositoryMock.Object);

        _passwordHashServiceMock
            .Setup(p => p.GenerateHash(content.Password))
            .Returns("hashed-password");

        userRepositoryMock
            .Setup(r => r.GetUserByCredentialsAsync(
                content,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(userInformations);

        var result = await _useCase.LoginAsync(content);

        Assert.True(result.IsT1);
        Assert.IsType<UserIsBannedError>(result.AsT1);

        _generateTokenServiceMock.Verify(
            g => g.GenerateRefreshToken(),
            Times.Never
        );
    }

    [Fact]
    public async Task LoginAsync_Should_Return_DatabaseError_When_UpsertSession_Fails()
    {
        var userId = Guid.NewGuid();

        var content = new RequestUserLoginDTO(
            "john@email.com",
            "password"
        );

        var userInformations = new UserCredentialsProjection(new RequestGenerateTokenDTO(
            userId,
            "John Doe",
            ERole.BIDDER,
            EUserStatus.ACTIVE
        ), "hashed-password");

        var refreshToken = new RefreshTokenDTO(
            "refresh-token",
            DateTime.UtcNow.AddDays(7)
        );

        var userRepositoryMock = new Mock<
            IUsersRepository
        >();

        var sessionRepositoryMock = new Mock<
            IUsersSessionRepository
        >();

        _unitOfWorkMock
            .SetupGet(u => u.UserRepository)
            .Returns(userRepositoryMock.Object);

        _unitOfWorkMock
            .SetupGet(u => u.UsersSessionRepository)
            .Returns(sessionRepositoryMock.Object);

        _passwordHashServiceMock
            .Setup(p => p.GenerateHash(content.Password))
            .Returns("hashed-password");

        userRepositoryMock
            .Setup(r => r.GetUserByCredentialsAsync(
                content,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(userInformations);

        _generateTokenServiceMock
            .Setup(g => g.GenerateRefreshToken())
            .Returns(refreshToken);

        sessionRepositoryMock
            .Setup(r => r.UpsertAsync(
                refreshToken,
                userId,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(false);

        var result = await _useCase.LoginAsync(content);

        Assert.True(result.IsT1);
        Assert.IsType<DatabaseError>(result.AsT1);

        _generateTokenServiceMock.Verify(
            g => g.GenerateToken(It.IsAny<RequestGenerateTokenDTO>()),
            Times.Never
        );
    }

    [Fact]
    public async Task LoginAsync_Should_Propagate_CancellationToken_To_Repositories()
    {
        var userId = Guid.NewGuid();
        var cancellationToken = new CancellationToken();

        var content = new RequestUserLoginDTO(
            "john@email.com",
            "password"
        );

        var userInformations = new UserCredentialsProjection(new RequestGenerateTokenDTO(
            userId,
            "John Doe",
            ERole.BIDDER,
            EUserStatus.ACTIVE
        ), "hashed-password");

        var refreshToken = new RefreshTokenDTO(
            "refresh-token",
            DateTime.UtcNow.AddDays(7)
        );

        var userRepositoryMock = new Mock<
            IUsersRepository
        >();

        var sessionRepositoryMock = new Mock<
            IUsersSessionRepository
        >();

        _unitOfWorkMock
            .SetupGet(u => u.UserRepository)
            .Returns(userRepositoryMock.Object);

        _unitOfWorkMock
            .SetupGet(u => u.UsersSessionRepository)
            .Returns(sessionRepositoryMock.Object);

        _passwordHashServiceMock
            .Setup(p => p.GenerateHash(content.Password))
            .Returns("hashed-password");

        userRepositoryMock
            .Setup(r => r.GetUserByCredentialsAsync(
                content,
                cancellationToken
            ))
            .ReturnsAsync(userInformations);

        _generateTokenServiceMock
            .Setup(g => g.GenerateRefreshToken())
            .Returns(refreshToken);

        sessionRepositoryMock
            .Setup(r => r.UpsertAsync(
                refreshToken,
                userId,
                cancellationToken
            ))
            .ReturnsAsync(true);

        _generateTokenServiceMock
            .Setup(g => g.GenerateToken(userInformations.User))
            .Returns("access-token");

        await _useCase.LoginAsync(content, cancellationToken);

        userRepositoryMock.Verify(
            r => r.GetUserByCredentialsAsync(
                content,
                cancellationToken
            ),
            Times.Once
        );

        sessionRepositoryMock.Verify(
            r => r.UpsertAsync(
                refreshToken,
                userId,
                cancellationToken
            ),
            Times.Once
        );
    }
}