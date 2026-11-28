using CrossCutting.Errors.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OneOf;
using System.Net;
using System.Net.Http.Json;
using User.Domain.Dto.Profile.Response;
using User.Domain.Dto.Update.Request;
using User.Domain.Enums;
using User.Domain.Errors.Common;
using User.Domain.Interfaces.UseCase.Profile.Queries;
using User.Domain.Interfaces.UseCase.Update.Commands;
using User.WebApi.Endpoints;

namespace User.UnitTests.WebApi;

public class UserProfileEndpointsTests
{
    [Fact]
    public async Task GetProfile_Should_Return_Ok_When_UseCaseSucceeds()
    {
        var profile = new ResponseUserProfileDTO(
            "Test User",
            "test@email.com",
            "Test bio",
            new DateOnly(2000, 1, 1),
            ERole.BIDDER,
            EUserStatus.ACTIVE
        );

        var useCaseMock = new Mock<IGetUserProfileUseCase>();

        useCaseMock
            .Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((OneOf<ResponseUserProfileDTO, CrossCutting.Errors.Base.BaseError>)profile);

        await using var app = await CreateAppAsync(services =>
        {
            services.AddSingleton(useCaseMock.Object);
        });

        using var client = app.GetTestClient();

        var response = await client.GetAsync("/profile");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseBody = await response.Content.ReadFromJsonAsync<ResponseUserProfileDTO>();

        Assert.NotNull(responseBody);
        Assert.Equal(profile.Name, responseBody.Name);
        Assert.Equal(profile.Email, responseBody.Email);
        Assert.Equal(profile.Bio, responseBody.Bio);
        Assert.Equal(profile.BirthDate, responseBody.BirthDate);
        Assert.Equal(profile.Role, responseBody.Role);

        useCaseMock.Verify(
            x => x.GetAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetProfile_Should_Return_ErrorStatusCode_When_UseCaseFails()
    {
        var expectedError = new UserNotFoundError();

        var useCaseMock = new Mock<IGetUserProfileUseCase>();

        useCaseMock
            .Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (OneOf<ResponseUserProfileDTO, CrossCutting.Errors.Base.BaseError>)expectedError
            );

        await using var app = await CreateAppAsync(services =>
        {
            services.AddSingleton(useCaseMock.Object);
        });

        using var client = app.GetTestClient();

        var response = await client.GetAsync("/profile");

        Assert.Equal(
            (HttpStatusCode)expectedError.HttpErrorCode,
            response.StatusCode
        );

        Assert.NotNull(await response.Content.ReadAsStringAsync());

        useCaseMock.Verify(
            x => x.GetAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateProfile_Should_Return_Ok_When_UseCaseSucceeds()
    {
        var request = new RequestUpdateProfileUserDTO(
            "Updated Name",
            "Updated bio"
        );

        var useCaseMock = new Mock<IUpdateProfileUserUseCase>();

        useCaseMock
            .Setup(x => x.UpdateAsync(
                It.IsAny<RequestUpdateProfileUserDTO>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((OneOf<bool, CrossCutting.Errors.Base.BaseError>)true);

        await using var app = await CreateAppAsync(services =>
        {
            services.AddSingleton(useCaseMock.Object);
        });

        using var client = app.GetTestClient();

        var response = await client.PatchAsJsonAsync("/profile", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseBody = await response.Content.ReadFromJsonAsync<bool>();

        Assert.True(responseBody);

        useCaseMock.Verify(
            x => x.UpdateAsync(
                It.Is<RequestUpdateProfileUserDTO>(value =>
                    value.Name == request.Name &&
                    value.Bio == request.Bio),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateProfile_Should_Return_ErrorStatusCode_When_UseCaseFails()
    {
        var request = new RequestUpdateProfileUserDTO(
            "Updated Name",
            "Updated bio"
        );

        var expectedError = new DatabaseError();

        var useCaseMock = new Mock<IUpdateProfileUserUseCase>();

        useCaseMock
            .Setup(x => x.UpdateAsync(
                It.IsAny<RequestUpdateProfileUserDTO>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (OneOf<bool, CrossCutting.Errors.Base.BaseError>)expectedError
            );

        await using var app = await CreateAppAsync(services =>
        {
            services.AddSingleton(useCaseMock.Object);
        });

        using var client = app.GetTestClient();

        var response = await client.PatchAsJsonAsync("/profile", request);

        Assert.Equal(
            (HttpStatusCode)expectedError.HttpErrorCode,
            response.StatusCode
        );

        useCaseMock.Verify(
            x => x.UpdateAsync(
                It.IsAny<RequestUpdateProfileUserDTO>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static async Task<WebApplication> CreateAppAsync(
        Action<IServiceCollection> configureServices)
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();

        configureServices(builder.Services);

        var app = builder.Build();

        app.MapUserProfileEndpoints();

        await app.StartAsync();

        return app;
    }
}