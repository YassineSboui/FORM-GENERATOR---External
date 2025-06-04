using Microsoft.Extensions.Primitives;
using NeoForm_Externe.Interfaces;
using Yarp.ReverseProxy.Configuration;

namespace NeoForm_Externe.Proxy
{
    public class CustomProxyConfigProvider : IProxyConfigProvider
    {
        private readonly IDynamicClientProvider _clientProvider;
        private CustomProxyConfig _config;
        private readonly CancellationTokenSource _cts = new();

        public CustomProxyConfigProvider(IDynamicClientProvider clientProvider)
        {
            _clientProvider = clientProvider;
            _config = BuildConfig();
        }

        public IProxyConfig GetConfig() => _config;

        public void UpdateDestination(string clusterId, string destinationUrl)
        {
            _config = BuildConfig();
            _cts.Cancel();
        }

        private CustomProxyConfig BuildConfig()
        {
            var routes = new List<RouteConfig>();
            var clusters = new List<ClusterConfig>();

            foreach (var client in _clientProvider.GetClients())
            {
                string clusterId = $"cluster-{client.Key}";

                routes.Add(new RouteConfig
                {
                    RouteId = $"route-{client.Key}",
                    ClusterId = clusterId,
                    Match = new RouteMatch
                    {
                        Path = $"/neoformexternal/{client.Key}/{{**catch-all}}"
                    },
                    Transforms = new[]
                    {
                        new Dictionary<string, string>
                        {
                            { "PathRemovePrefix", $"/neoformexternal/{client.Key}" }
                        }
                    }
                });

                clusters.Add(new ClusterConfig
                {
                    ClusterId = clusterId,
                    Destinations = new Dictionary<string, DestinationConfig>
                    {
                        {
                            "default", new DestinationConfig
                            {
                                Address = client.Value
                            }
                        }
                    }
                });
            }

            return new CustomProxyConfig(routes, clusters);
        }

        private class CustomProxyConfig : IProxyConfig
        {
            public CustomProxyConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
            {
                Routes = routes;
                Clusters = clusters;
                ChangeToken = new CancellationChangeToken(new CancellationTokenSource().Token);
            }

            public IReadOnlyList<RouteConfig> Routes { get; }
            public IReadOnlyList<ClusterConfig> Clusters { get; }
            public IChangeToken ChangeToken { get; }
        }
    }
}
