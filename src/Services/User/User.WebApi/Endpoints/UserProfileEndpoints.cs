using CrossCutting.Errors.Base;
using Microsoft.AspNetCore.Mvc;
using User.Domain.Dto.Profile;
using User.Domain.Interfaces.UseCase.Profile.Queries;

namespace User.WebApi.Endpoints
{
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
                .Produces<UserProfileResponseDTO>(StatusCodes.Status200OK)
                .Produces<BaseError>(StatusCodes.Status403Forbidden)
                .Produces<BaseError>(StatusCodes.Status404NotFound);

            return endpoints;
        }
    }
}
