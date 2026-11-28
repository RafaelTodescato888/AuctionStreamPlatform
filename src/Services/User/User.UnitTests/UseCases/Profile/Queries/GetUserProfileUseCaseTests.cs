using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Security.Claims;
using User.Application.UseCases.Profile.Queries;
using User.Domain.Dto.Profile.Response;
using User.Domain.Enums;
using User.Domain.Errors.Common;
using User.Domain.Errors.Common.Authentication;
using User.Domain.Interfaces.Repositories.UserProfile;
using User.Domain.Interfaces.UoW;

namespace User.UnitTests.UseCases.Profile.Queries;

public class GetUserProfileUseCaseTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ILogger<GetUserProfileUseCase> _logger;
    private readonly Mock<IUsersProfileRepository> _usersProfileRepositoryMock;

    private readonly GetUserProfileUseCase _useCase;

    public GetUserProfileUseCaseTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _logger = NullLogger<GetUserProfileUseCase>.Instance;
        _usersProfileRepositoryMock = new Mock<IUsersProfileRepository>();

        _unitOfWorkMock
            .SetupGet(u => u.UsersProfileRepository)
            .Returns(_usersProfileRepositoryMock.Object);

        _useCase = new GetUserProfileUseCase(
            _httpContextAccessorMock.Object,
            _unitOfWorkMock.Object,
            _logger
        );
    }

    [Fact]
    public async Task GetAsync_Should_Return_UserProfile_When_User_Is_Authorized()
    {
        var userId = Guid.NewGuid();

        var expectedProfile = new ResponseUserProfileDTO(
            "John Doe",
            "john@email.com",
            "Software Developer",
            new DateOnly(2000, 1, 1),
            ERole.BIDDER,
            EUserStatus.ACTIVE
        );

        var httpContext = CreateAuthenticatedHttpContext(userId);

        _httpContextAccessorMock
            .SetupGet(h => h.HttpContext)
            .Returns(httpContext);

        _usersProfileRepositoryMock
            .Setup(r => r.GetAsync(
                userId,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(expectedProfile);

        var result = await _useCase.GetAsync();

        Assert.True(result.IsT0);

        var response = result.AsT0;

        Assert.Equal(expectedProfile.Name, response.Name);
        Assert.Equal(expectedProfile.Email, response.Email);
        Assert.Equal(expectedProfile.Bio, response.Bio);
        Assert.Equal(expectedProfile.BirthDate, response.BirthDate);
        Assert.Equal(expectedProfile.Role, response.Role);

        _usersProfileRepositoryMock.Verify(
            r => r.GetAsync(
                userId,
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAsync_Should_Return_UserIsNotAuthorizedError_When_HttpContext_Is_Null()
    {
        _httpContextAccessorMock
            .SetupGet(h => h.HttpContext)
            .Returns((HttpContext?)null);

        var result = await _useCase.GetAsync();

        Assert.True(result.IsT1);
        Assert.IsType<UserIsNotAuthorizedError>(result.AsT1);

        _usersProfileRepositoryMock.Verify(
            r => r.GetAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
    }

    [Fact]
    public async Task GetAsync_Should_Return_UserIsNotAuthorizedError_When_User_Is_Not_Authenticated()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity())
        };

        _httpContextAccessorMock
            .SetupGet(h => h.HttpContext)
            .Returns(httpContext);

        var result = await _useCase.GetAsync();

        Assert.True(result.IsT1);
        Assert.IsType<UserIsNotAuthorizedError>(result.AsT1);

        _usersProfileRepositoryMock.Verify(
            r => r.GetAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
    }

    [Fact]
    public async Task GetAsync_Should_Return_UserIsNotAuthorizedError_When_NameIdentifier_Is_Missing()
    {
        var userId = Guid.NewGuid();

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "John Doe")
        };

        var identity = new ClaimsIdentity(
            claims,
            "TestAuthentication"
        );

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };

        _httpContextAccessorMock
            .SetupGet(h => h.HttpContext)
            .Returns(httpContext);

        var result = await _useCase.GetAsync();

        Assert.True(result.IsT1);
        Assert.IsType<UserIsNotAuthorizedError>(result.AsT1);

        _usersProfileRepositoryMock.Verify(
            r => r.GetAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
    }

    [Fact]
    public async Task GetAsync_Should_Return_UserNotFoundError_When_Profile_Does_Not_Exist()
    {
        var userId = Guid.NewGuid();

        var httpContext = CreateAuthenticatedHttpContext(userId);

        _httpContextAccessorMock
            .SetupGet(h => h.HttpContext)
            .Returns(httpContext);

        _usersProfileRepositoryMock
            .Setup(r => r.GetAsync(
                userId,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((ResponseUserProfileDTO?)null);

        var result = await _useCase.GetAsync();

        Assert.True(result.IsT1);
        Assert.IsType<UserNotFoundError>(result.AsT1);

        _usersProfileRepositoryMock.Verify(
            r => r.GetAsync(
                userId,
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAsync_Should_Propagate_CancellationToken_To_Repository()
    {
        var userId = Guid.NewGuid();
        var cancellationToken = new CancellationToken();

        var expectedProfile = new ResponseUserProfileDTO(
            "John Doe",
            "john@email.com",
            null,
            new DateOnly(2000, 1, 1),
            ERole.ADMIN,
            EUserStatus.ACTIVE
        );

        var httpContext = CreateAuthenticatedHttpContext(userId);

        _httpContextAccessorMock
            .SetupGet(h => h.HttpContext)
            .Returns(httpContext);

        _usersProfileRepositoryMock
            .Setup(r => r.GetAsync(
                userId,
                cancellationToken
            ))
            .ReturnsAsync(expectedProfile);

        var result = await _useCase.GetAsync(cancellationToken);

        Assert.True(result.IsT0);

        _usersProfileRepositoryMock.Verify(
            r => r.GetAsync(
                userId,
                cancellationToken
            ),
            Times.Once
        );
    }

    private static DefaultHttpContext CreateAuthenticatedHttpContext(Guid userId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, "John Doe")
        };

        var identity = new ClaimsIdentity(
            claims,
            "TestAuthentication"
        );

        return new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };
    }
}