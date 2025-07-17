using Microsoft.Extensions.Primitives;
using NeoForm_Externe.Interfaces;
using Yarp.ReverseProxy.Configuration;
using System.Collections.Concurrent;

namespace NeoForm_Externe.Proxy
{
    public class CustomProxyConfigProvider : IProxyConfigProvider
    {
        private readonly IDynamicClientProvider _clientProvider;
        private readonly ILogger<CustomProxyConfigProvider> _logger;
        private readonly IWebHostEnvironment _env;
        private volatile CustomProxyConfig _config;
        private readonly CancellationTokenSource _cts = new();
        private readonly object _lock = new();

        public CustomProxyConfigProvider(IDynamicClientProvider clientProvider, ILogger<CustomProxyConfigProvider> logger, IWebHostEnvironment env)
        {
            _clientProvider = clientProvider;
            _logger = logger;
            _env = env;
            _config = BuildConfig();
        }

        public IProxyConfig GetConfig() => _config;

        public void UpdateDestination(string clusterId, string destinationUrl)
        {
            lock (_lock)
            {
                _config = BuildConfig();
                _cts.Cancel();
                _logger.LogInformation($"Proxy configuration updated for cluster {clusterId}");
            }
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

                // Clean the client URL by removing /neoform suffix to avoid double paths (only in development)
                var cleanedUrl = client.Value.TrimEnd('/');
                if (_env.IsDevelopment())
                {
                    if (cleanedUrl.EndsWith("/neoform", StringComparison.OrdinalIgnoreCase))
                    {
                        cleanedUrl = cleanedUrl.Substring(0, cleanedUrl.Length - 8); // Remove "/neoform"
                    }
                }

                _logger.LogDebug($"Proxy destination for client {client.Key}: original='{client.Value}', cleaned='{cleanedUrl}'");

                clusters.Add(new ClusterConfig
                {
                    ClusterId = clusterId,
                    LoadBalancingPolicy = "RoundRobin",
                    // Temporarily disable health checks since backend services may not be running
                    /*
                    HealthCheck = new HealthCheckConfig
                    {
                        Active = new ActiveHealthCheckConfig
                        {
                            Enabled = true,
                            Interval = TimeSpan.FromMinutes(1),
                            Timeout = TimeSpan.FromSeconds(30),
                            Policy = "ConsecutiveFailures",
                            Path = "/health"
                        }
                    },
                    */
                    Destinations = new Dictionary<string, DestinationConfig>
                    {
                        {
                            "default", new DestinationConfig
                            {
                                Address = cleanedUrl,
                                // Health = client.Value + "/health"
                            }
                        }
                    }
                });
            }

            _logger.LogDebug($"Built proxy configuration with {routes.Count} routes and {clusters.Count} clusters");
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
