using Gateway.WebApi.Extensions.ReverseProxy.Service.User.Cluster;
using Yarp.ReverseProxy.Configuration;

namespace Gateway.WebApi.Extensions.ReverseProxy
{
    internal static class ClusterExtensions
    {
        internal static IReadOnlyList<ClusterConfig> ConfigureClusters()
        {
            var clusters = new List<ClusterConfig>();

            return clusters.ConfigureUserCluster();
        }
    }
}
