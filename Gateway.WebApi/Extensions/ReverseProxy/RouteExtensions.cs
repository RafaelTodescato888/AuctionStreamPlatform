using Gateway.WebApi.Extensions.ReverseProxy.Service.User.Routes;
using Yarp.ReverseProxy.Configuration;

namespace Gateway.WebApi.Extensions.ReverseProxy
{
    internal static class RouteExtensions
    {
        internal static IReadOnlyList<RouteConfig> ConfigureRoutes()
        {
            var routes = new List<RouteConfig>();

            return routes.ConfigureUserRoutes();
        }
    }
}
