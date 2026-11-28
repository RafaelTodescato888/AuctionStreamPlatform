using System.Net;
using System.Net.Http.Json;
using CrossCutting.Errors.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OneOf;
using User.Domain.Dto.Login.Request;
using User.Domain.Dto.Login.Response;
using User.Domain.Dto.Register.Request;
using User.Domain.Interfaces.UseCase.Login.Commands;
using User.Domain.Interfaces.UseCase.Register.Bidder.Commands;
using User.Domain.Interfaces.UseCase.Register.Seller.Commands;
using User.WebApi.Endpoints;

namespace User.UnitTests.WebApi.Endpoints;

public class AuthenticationEndpointsTests
{
    [Fact]
    public async Task RegisterBidder_Should_Return_Ok_When_UseCaseSucceeds()
    {
        var request = CreateValidRegisterRequest();

        var useCaseMock = new Mock<IRegisterBidderUseCase>();

        useCaseMock
            .Setup(x => x.RegisterAsync(
                It.IsAny<RequestRegisterUserDTO>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((OneOf<bool, BaseError>)true);

        await using var app = await CreateAppAsync(services =>
        {
            services.AddSingleton(useCaseMock.Object);
        });

        using var client = app.GetTestClient();

        var response = await client.PostAsJsonAsync(
            "/auth/register/bidder",
            request
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(await response.Content.ReadFromJsonAsync<bool>());

        useCaseMock.Verify(
            x => x.RegisterAsync(
                It.Is<RequestRegisterUserDTO>(value =>
                    value.Name == request.Name &&
                    value.Email == request.Email &&
                    value.Document == request.Document),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterBidder_Should_Return_ErrorStatusCode_When_UseCaseFails()
    {
        var request = CreateValidRegisterRequest();
        var expectedError = new DatabaseError();

        var useCaseMock = new Mock<IRegisterBidderUseCase>();

        useCaseMock
            .Setup(x => x.RegisterAsync(
                It.IsAny<RequestRegisterUserDTO>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((OneOf<bool, BaseError>)expectedError);

        await using var app = await CreateAppAsync(services =>
        {
            services.AddSingleton(useCaseMock.Object);
        });

        using var client = app.GetTestClient();

        var response = await client.PostAsJsonAsync(
            "/auth/register/bidder",
            request
        );

        Assert.Equal(
            (HttpStatusCode)expectedError.HttpErrorCode,
            response.StatusCode
        );

        useCaseMock.Verify(
            x => x.RegisterAsync(
                It.IsAny<RequestRegisterUserDTO>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterSeller_Should_Return_Ok_When_UseCaseSucceeds()
    {
        var request = CreateValidRegisterRequest();

        var useCaseMock = new Mock<IRegisterSellerUseCase>();

        useCaseMock
            .Setup(x => x.RegisterAsync(
                It.IsAny<RequestRegisterUserDTO>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((OneOf<bool, BaseError>)true);

        await using var app = await CreateAppAsync(services =>
        {
            services.AddSingleton(useCaseMock.Object);
        });

        using var client = app.GetTestClient();

        var response = await client.PostAsJsonAsync(
            "/auth/register/seller",
            request
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(await response.Content.ReadFromJsonAsync<bool>());

        useCaseMock.Verify(
            x => x.RegisterAsync(
                It.Is<RequestRegisterUserDTO>(value =>
                    value.Name == request.Name &&
                    value.Email == request.Email &&
                    value.Document == request.Document),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterSeller_Should_Return_ErrorStatusCode_When_UseCaseFails()
    {
        var request = CreateValidRegisterRequest();
        var expectedError = new DatabaseError();

        var useCaseMock = new Mock<IRegisterSellerUseCase>();

        useCaseMock
            .Setup(x => x.RegisterAsync(
                It.IsAny<RequestRegisterUserDTO>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((OneOf<bool, BaseError>)expectedError);

        await using var app = await CreateAppAsync(services =>
        {
            services.AddSingleton(useCaseMock.Object);
        });

        using var client = app.GetTestClient();

        var response = await client.PostAsJsonAsync(
            "/auth/register/seller",
            request
        );

        Assert.Equal(
            (HttpStatusCode)expectedError.HttpErrorCode,
            response.StatusCode
        );

        useCaseMock.Verify(
            x => x.RegisterAsync(
                It.IsAny<RequestRegisterUserDTO>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Login_Should_Return_Ok_When_UseCaseSucceeds()
    {
        var request = new RequestUserLoginDTO(
            "test@email.com",
            "password"
        );

        var loginResponse = new ResponseUserLoginDTO(
            "access-token",
            "refresh-token"
        );

        var useCaseMock = new Mock<ILoginUseCase>();

        useCaseMock
            .Setup(x => x.LoginAsync(
                It.IsAny<RequestUserLoginDTO>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((OneOf<ResponseUserLoginDTO, BaseError>)loginResponse);

        await using var app = await CreateAppAsync(services =>
        {
            services.AddSingleton(useCaseMock.Object);
        });

        using var client = app.GetTestClient();

        var response = await client.PostAsJsonAsync(
            "/auth/login",
            request
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseBody = await response.Content.ReadFromJsonAsync<ResponseUserLoginDTO>();

        Assert.NotNull(responseBody);
        Assert.Equal(loginResponse.Token, responseBody.Token);
        Assert.Equal(loginResponse.RefreshToken, responseBody.RefreshToken);

        useCaseMock.Verify(
            x => x.LoginAsync(
                It.Is<RequestUserLoginDTO>(value =>
                    value.Email == request.Email &&
                    value.Password == request.Password),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Login_Should_Return_ErrorStatusCode_When_UseCaseFails()
    {
        var request = new RequestUserLoginDTO(
            "test@email.com",
            "wrong-password"
        );

        var expectedError = new DatabaseError();

        var useCaseMock = new Mock<ILoginUseCase>();

        useCaseMock
            .Setup(x => x.LoginAsync(
                It.IsAny<RequestUserLoginDTO>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((OneOf<ResponseUserLoginDTO, BaseError>)expectedError);

        await using var app = await CreateAppAsync(services =>
        {
            services.AddSingleton(useCaseMock.Object);
        });

        using var client = app.GetTestClient();

        var response = await client.PostAsJsonAsync(
            "/auth/login",
            request
        );

        Assert.Equal(
            (HttpStatusCode)expectedError.HttpErrorCode,
            response.StatusCode
        );

        useCaseMock.Verify(
            x => x.LoginAsync(
                It.IsAny<RequestUserLoginDTO>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static RequestRegisterUserDTO CreateValidRegisterRequest()
    {
        return new RequestRegisterUserDTO(
            "Test User",
            "test@email.com",
            "Test bio",
            new DateOnly(2000, 1, 1),
            "52998224725",
            "password"
        );
    }

    private static async Task<WebApplication> CreateAppAsync(
        Action<IServiceCollection> configureServices)
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();

        configureServices(builder.Services);

        var app = builder.Build();

        app.MapAuthenticationEndpoints();

        await app.StartAsync();

        return app;
    }
}