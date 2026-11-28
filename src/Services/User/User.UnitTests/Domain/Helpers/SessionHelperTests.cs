using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using User.Domain.Helpers;

namespace User.UnitTests.Domain.Helpers;

public class SessionHelperTests
{
    [Fact]
    public void GetUserId_Should_Return_UserId_When_User_Is_Authenticated()
    {
        var userId = Guid.NewGuid();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };

        var identity = new ClaimsIdentity(claims, "TestAuthentication");

        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        var result = SessionHelper.GetUserId(httpContext);

        Assert.Equal(userId, result);
    }

    [Fact]
    public void GetUserId_Should_Return_Null_When_HttpContext_Is_Null()
    {
        var result = SessionHelper.GetUserId(null);

        Assert.Null(result);
    }

    [Fact]
    public void GetUserId_Should_Return_Null_When_User_Is_Not_Authenticated()
    {
        var identity = new ClaimsIdentity();

        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        var result = SessionHelper.GetUserId(httpContext);

        Assert.Null(result);
    }

    [Fact]
    public void GetUserId_Should_Return_Null_When_NameIdentifier_Claim_Is_Missing()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "John Doe")
        };

        var identity = new ClaimsIdentity(claims, "TestAuthentication");

        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        var result = SessionHelper.GetUserId(httpContext);

        Assert.Null(result);
    }

    [Fact]
    public void GetUserId_Should_Return_Null_When_NameIdentifier_Is_Not_A_Valid_Guid()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "invalid-guid")
        };

        var identity = new ClaimsIdentity(claims, "TestAuthentication");

        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        var result = SessionHelper.GetUserId(httpContext);

        Assert.Null(result);
    }

    [Fact]
    public void GetUserId_Should_Return_Null_When_NameIdentifier_Is_Empty()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, string.Empty)
        };

        var identity = new ClaimsIdentity(claims, "TestAuthentication");

        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        var result = SessionHelper.GetUserId(httpContext);

        Assert.Null(result);
    }
}