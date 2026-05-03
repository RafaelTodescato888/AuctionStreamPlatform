using Yarp.ReverseProxy.Configuration;

namespace Gateway.WebApi.Extensions.ReverseProxy.Service.User.Cluster
{
    internal static class ClusterExtensions
    {
        internal static List<ClusterConfig> ConfigureUserCluster(this List<ClusterConfig> clusters)
        {
            clusters.Add(new ClusterConfig
            {
                ClusterId = "user-cluster",
                Destinations = new Dictionary<string, DestinationConfig>
                {
                    { "destionationUrl", new DestinationConfig { Address = "http://user.webapi:8080/" } }
                }
            });

            return clusters;
        }
    }
}
