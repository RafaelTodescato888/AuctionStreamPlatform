using CrossCutting.Errors.Base;
using Microsoft.AspNetCore.Mvc;
using User.Domain.Constants.RateLimitStrategy;
using User.Domain.Dto.Login.Request;
using User.Domain.Dto.Login.Response;
using User.Domain.Dto.Register.Request;
using User.Domain.Interfaces.UseCase.Login.Commands;
using User.Domain.Interfaces.UseCase.Register.Bidder.Commands;
using User.Domain.Interfaces.UseCase.Register.Seller.Commands;

namespace User.WebApi.Endpoints;

internal static class AuthenticationEndpoints
{
    internal static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var root = endpoints.MapGroup("auth")
            .WithTags();

        root.MapPost("register/bidder", async (
            [FromServices] IRegisterBidderUseCase useCase,
            [FromBody] RequestRegisterUserDTO request,
            CancellationToken cancellationToken = default
        ) =>
        {
            var result = await useCase.RegisterAsync(request, cancellationToken);

            return result.Match(
                success => Results.Ok(success),
                error => Results.Json(error, statusCode: error.HttpErrorCode)
            );
        })
            .WithDescription("Creates a bidder.")
            .Produces<bool>(StatusCodes.Status200OK)
            .Produces<BaseError>(StatusCodes.Status400BadRequest)
            .Produces<BaseError>(StatusCodes.Status409Conflict)
            .Produces<BaseError>(StatusCodes.Status500InternalServerError);

        root.MapPost("register/seller", async (
            [FromServices] IRegisterSellerUseCase useCase,
            [FromBody] RequestRegisterUserDTO request,
            CancellationToken cancellationToken = default
        ) =>
        {
            var result = await useCase.RegisterAsync(request, cancellationToken);
            return result.Match(
                success => Results.Ok(success),
                error => Results.Json(error, statusCode: error.HttpErrorCode)
            );
        })
            .WithDescription("Creates a seller.")
            .Produces<bool>(StatusCodes.Status200OK)
            .Produces<BaseError>(StatusCodes.Status400BadRequest)
            .Produces<BaseError>(StatusCodes.Status409Conflict)
            .Produces<BaseError>(StatusCodes.Status500InternalServerError);

        root.MapPost("login", async (
            [FromServices] ILoginUseCase useCase,
            [FromBody] RequestUserLoginDTO request,
            CancellationToken cancellationToken = default
        ) =>
        {
            var result = await useCase.LoginAsync(request, cancellationToken);

            return result.Match(
                success => Results.Ok(success),
                error => Results.Json(error, statusCode: error.HttpErrorCode)
            );
        })
            .WithDescription("Logs in an user.")
            .Produces<ResponseUserLoginDTO>(StatusCodes.Status200OK)
            .Produces<BaseError>(StatusCodes.Status400BadRequest)
            .Produces<BaseError>(StatusCodes.Status401Unauthorized)
            .Produces<BaseError>(StatusCodes.Status500InternalServerError)
            .RequireRateLimiting(RateLimitStrategyConstant.Login_Fixed_Window);

        return endpoints;
    }
}
