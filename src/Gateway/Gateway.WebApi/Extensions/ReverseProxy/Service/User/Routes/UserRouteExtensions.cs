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
                },
                new RouteConfig {
                    RouteId = "user-login",
                    ClusterId = "user-cluster",
                    Match = new RouteMatch
                    {
                        Path = "/api/users/auth/login",
                        Methods = ["POST"]
                    },
                    Transforms = [
                        new Dictionary<string, string> {
                            { "PathPattern", "auth/login"}
                        }
                    ]
                },
                new RouteConfig
                {
                    RouteId = "user-profile",
                    ClusterId = "user-cluster",
                    Match = new RouteMatch
                    {
                        Path = "/api/users/profile/{**catch-all}",
                        Methods = ["GET", "PATCH"]
                    },
                    Transforms = [
                        new Dictionary<string, string> {
                            { "PathPattern", "profile/{**catch-all}"}
                        }
                    ]
                }
            ]);

            return routes;
        }
    }
}
