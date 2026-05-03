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
                    RouteId = "user-weatherforecast",
                    ClusterId = "user-cluster",
                    Match = new RouteMatch
                    {
                        Path = "/api/user/weatherforecast",
                        Methods = ["GET"]
                    },
                    Transforms = [
                        new Dictionary<string, string> {
                            { "PathPattern", "weatherforecast"}
                        }
                    ]
                }
            ]);

            return routes;
        }
    }
}
