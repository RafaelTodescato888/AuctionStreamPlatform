using Yarp.ReverseProxy.Configuration;

namespace Gateway.WebApi.Extensions.ReverseProxy.Service.User.Routes
{
    internal static class UserRouteExtensions
    {
        internal static List<RouteConfig> ConfigureUserRoutes(this List<RouteConfig> routes)
        {
            routes.AddRange(
            [
                new RouteConfig
                {
                    RouteId = "user-create",
                    ClusterId = "user-cluster",
                    Match = new RouteMatch
                    {
                        Path = "/api/users/auth/register",
                        Methods = ["POST"]
                    },
                    Transforms = [
                        new Dictionary<string, string> {
                            { "PathPattern", "auth/register"}
                        }
                    ]
                }
            ]);

            return routes;
        }
    }
}
