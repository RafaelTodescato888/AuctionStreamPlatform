using User.WebApi.Endpoints;

namespace User.WebApi.Extensions.Endpoints
{
    internal static class ConfigureEndpointsExtensions
    {
        internal static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder endpoints)
        {
            return endpoints.MapAuthenticationEndpoints();
        }
    }
}
