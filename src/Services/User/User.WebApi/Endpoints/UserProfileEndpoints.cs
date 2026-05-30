using CrossCutting.Errors.Base;
using Microsoft.AspNetCore.Mvc;
using User.Domain.Dto.Profile;
using User.Domain.Dto.Update.Request;
using User.Domain.Interfaces.UseCase.Profile.Queries;
using User.Domain.Interfaces.UseCase.Update.Commands;

namespace User.WebApi.Endpoints;

internal static class UserProfileEndpoints
{
    internal static IEndpointRouteBuilder MapUserProfileEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var root = endpoints.MapGroup("profile")
            .WithTags();

        root.MapGet("", async (
            [FromServices] IGetUserProfileUseCase useCase,
            CancellationToken cancellationToken = default
        ) =>
        {
            var result = await useCase.GetAsync(cancellationToken);

            return result.Match(
                profile => Results.Ok(profile),
                error => Results.Json(error, statusCode: error.HttpErrorCode)
            );
        })
            .WithDescription("Get current logged user informations.")
            .Produces<ResponseUserProfileDTO>(StatusCodes.Status200OK)
            .Produces<BaseError>(StatusCodes.Status403Forbidden)
            .Produces<BaseError>(StatusCodes.Status404NotFound);

        root.MapPatch("", async (
            [FromServices] IUpdateProfileUserUseCase useCase,
            [FromBody] RequestUpdateProfileUserDTO request,
            CancellationToken cancellationToken = default
        ) =>
        {
            var result = await useCase.UpdateAsync(request, cancellationToken);

            return result.Match(
                success => Results.Ok(success),
                error => Results.Json(error, statusCode: error.HttpErrorCode)
            );
        })
            .WithDescription("Update user profile informations.")
            .Produces<bool>(StatusCodes.Status200OK)
            .Produces<BaseError>(StatusCodes.Status400BadRequest)
            .Produces<BaseError>(StatusCodes.Status404NotFound)
            .Produces<BaseError>(StatusCodes.Status500InternalServerError);

        return endpoints;
    }
}
