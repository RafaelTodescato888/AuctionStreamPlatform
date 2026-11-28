using System.Security.Claims;
using CrossCutting.Errors.Base;
using Microsoft.AspNetCore.Http;
using Moq;
using OneOf;
using User.Application.UseCases.Update.Commands;
using User.Domain.Dto.Update.Request;
using User.Domain.Errors.Common;
using User.Domain.Errors.Common.Authentication;
using User.Domain.Interfaces.Repositories.UserProfile;
using User.Domain.Interfaces.UoW;

namespace User.UnitTests.UseCases.Update.Commands;

public class UpdateProfileUserUseCaseTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUsersProfileRepository> _usersProfileRepositoryMock = new();

    private readonly UpdateProfileUserUseCase _useCase;

    public UpdateProfileUserUseCaseTests()
    {
        _unitOfWorkMock
            .SetupGet(x => x.UsersProfileRepository)
            .Returns(_usersProfileRepositoryMock.Object);

        _useCase = new UpdateProfileUserUseCase(
            _httpContextAccessorMock.Object,
            _unitOfWorkMock.Object
        );
    }

    // -------------------------------------------------------
    // Authentication
    // -------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_Should_Return_UserIsNotAuthorizedError_When_HttpContextIsNull()
    {
        _httpContextAccessorMock
            .SetupGet(x => x.HttpContext)
            .Returns((HttpContext?)null);

        var request = CreateValidRequest();

        var result = await _useCase.UpdateAsync(request, CancellationToken.None);

        Assert.True(result.IsT1);
        Assert.IsType<UserIsNotAuthorizedError>(result.AsT1);

        VerifyUpdateWasNotStarted();
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_UserIsNotAuthorizedError_When_UserIsNotAuthenticated()
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
                new ClaimsIdentity()
            )
        };

        _httpContextAccessorMock
            .SetupGet(x => x.HttpContext)
            .Returns(context);

        var request = CreateValidRequest();

        var result = await _useCase.UpdateAsync(request, CancellationToken.None);

        Assert.True(result.IsT1);
        Assert.IsType<UserIsNotAuthorizedError>(result.AsT1);

        VerifyUpdateWasNotStarted();
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_UserIsNotAuthorizedError_When_UserIdClaimIsMissing()
    {
        var context = CreateAuthenticatedHttpContext(
            Guid.NewGuid(),
            includeUserId: false
        );

        _httpContextAccessorMock
            .SetupGet(x => x.HttpContext)
            .Returns(context);

        var request = CreateValidRequest();

        var result = await _useCase.UpdateAsync(request, CancellationToken.None);

        Assert.True(result.IsT1);
        Assert.IsType<UserIsNotAuthorizedError>(result.AsT1);

        VerifyUpdateWasNotStarted();
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_UserIsNotAuthorizedError_When_UserIdClaimIsInvalid()
    {
        var context = CreateAuthenticatedHttpContext(
            Guid.Empty,
            userIdValue: "invalid-guid"
        );

        _httpContextAccessorMock
            .SetupGet(x => x.HttpContext)
            .Returns(context);

        var request = CreateValidRequest();

        var result = await _useCase.UpdateAsync(request, CancellationToken.None);

        Assert.True(result.IsT1);
        Assert.IsType<UserIsNotAuthorizedError>(result.AsT1);

        VerifyUpdateWasNotStarted();
    }

    // -------------------------------------------------------
    // Validation
    // -------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_Should_Return_NameIsRequiredError_When_NameIsEmpty()
    {
        var userId = Guid.NewGuid();

        SetupAuthenticatedUser(userId);

        var request = CreateValidRequest(name: "");

        var result = await _useCase.UpdateAsync(request, CancellationToken.None);

        Assert.True(result.IsT1);
        Assert.IsType<NameIsRequiredError>(result.AsT1);

        _usersProfileRepositoryMock.Verify(
            x => x.ExistsByUserIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _usersProfileRepositoryMock.Verify(
            x => x.UpdateAsync(
                It.IsAny<RequestUpdateProfileUserDTO>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task UpdateAsync_Should_Return_NameIsRequiredError_When_NameIsWhitespace(
        string name)
    {
        var userId = Guid.NewGuid();

        SetupAuthenticatedUser(userId);

        var request = CreateValidRequest(name: name);

        var result = await _useCase.UpdateAsync(request, CancellationToken.None);

        Assert.True(result.IsT1);
        Assert.IsType<NameIsRequiredError>(result.AsT1);

        _usersProfileRepositoryMock.Verify(
            x => x.ExistsByUserIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_UserNotFoundError_When_UserDoesNotExist()
    {
        var userId = Guid.NewGuid();

        SetupAuthenticatedUser(userId);

        var request = CreateValidRequest();

        _usersProfileRepositoryMock
            .Setup(x => x.ExistsByUserIdAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _useCase.UpdateAsync(request, CancellationToken.None);

        Assert.True(result.IsT1);
        Assert.IsType<UserNotFoundError>(result.AsT1);

        _usersProfileRepositoryMock.Verify(
            x => x.UpdateAsync(
                It.IsAny<RequestUpdateProfileUserDTO>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // -------------------------------------------------------
    // Successful update
    // -------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_Should_Return_True_When_ProfileIsUpdated()
    {
        var userId = Guid.NewGuid();

        SetupAuthenticatedUser(userId);

        var request = CreateValidRequest();

        _usersProfileRepositoryMock
            .Setup(x => x.ExistsByUserIdAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _usersProfileRepositoryMock
            .Setup(x => x.UpdateAsync(
                request,
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _useCase.UpdateAsync(request, CancellationToken.None);

        Assert.True(result.IsT0);
        Assert.True(result.AsT0);

        _usersProfileRepositoryMock.Verify(
            x => x.ExistsByUserIdAsync(
                userId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _usersProfileRepositoryMock.Verify(
            x => x.UpdateAsync(
                request,
                userId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_DatabaseError_When_UpdateFails()
    {
        var userId = Guid.NewGuid();

        SetupAuthenticatedUser(userId);

        var request = CreateValidRequest();

        _usersProfileRepositoryMock
            .Setup(x => x.ExistsByUserIdAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _usersProfileRepositoryMock
            .Setup(x => x.UpdateAsync(
                request,
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _useCase.UpdateAsync(request, CancellationToken.None);

        Assert.True(result.IsT1);
        Assert.IsType<DatabaseError>(result.AsT1);
    }

    // -------------------------------------------------------
    // Cancellation
    // -------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_Should_Propagate_CancellationToken_To_Repositories()
    {
        var userId = Guid.NewGuid();
        var cancellationToken = new CancellationTokenSource().Token;

        SetupAuthenticatedUser(userId);

        var request = CreateValidRequest();

        _usersProfileRepositoryMock
            .Setup(x => x.ExistsByUserIdAsync(
                userId,
                cancellationToken))
            .ReturnsAsync(true);

        _usersProfileRepositoryMock
            .Setup(x => x.UpdateAsync(
                request,
                userId,
                cancellationToken))
            .ReturnsAsync(true);

        await _useCase.UpdateAsync(request, cancellationToken);

        _usersProfileRepositoryMock.Verify(
            x => x.ExistsByUserIdAsync(
                userId,
                cancellationToken),
            Times.Once);

        _usersProfileRepositoryMock.Verify(
            x => x.UpdateAsync(
                request,
                userId,
                cancellationToken),
            Times.Once);
    }

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

    private void SetupAuthenticatedUser(Guid userId)
    {
        var context = CreateAuthenticatedHttpContext(userId);

        _httpContextAccessorMock
            .SetupGet(x => x.HttpContext)
            .Returns(context);
    }

    private static DefaultHttpContext CreateAuthenticatedHttpContext(
        Guid userId,
        bool includeUserId = true,
        string? userIdValue = null)
    {
        var claims = new List<Claim>();

        if (includeUserId)
        {
            claims.Add(new Claim(
                ClaimTypes.NameIdentifier,
                userIdValue ?? userId.ToString()
            ));
        }

        var identity = new ClaimsIdentity(
            claims,
            "TestAuthentication"
        );

        return new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };
    }

    private void VerifyUpdateWasNotStarted()
    {
        _usersProfileRepositoryMock.Verify(
            x => x.ExistsByUserIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _usersProfileRepositoryMock.Verify(
            x => x.UpdateAsync(
                It.IsAny<RequestUpdateProfileUserDTO>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static RequestUpdateProfileUserDTO CreateValidRequest(
        string name = "Updated Name",
        string? bio = "Updated bio")
    {
        return new RequestUpdateProfileUserDTO(name, bio);
    }
}