using NeoForm_Externe.Services.Authentication;
using NeoForm_Externe.Services.Client;

namespace NeoForm_Externe.Services.Background
{
    public class CleanupBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CleanupBackgroundService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(30);

        public CleanupBackgroundService(IServiceProvider serviceProvider, ILogger<CleanupBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Cleanup background service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Run(() => CleanupExpiredData());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during cleanup process");
                }

                await Task.Delay(_cleanupInterval, stoppingToken);
            }

            _logger.LogInformation("Cleanup background service stopped");
        }

        private Task CleanupExpiredData()
        {
            using var scope = _serviceProvider.CreateScope();

            try
            {
                // Clean expired tokens
                var tokenService = scope.ServiceProvider.GetService<TokenService>();
                tokenService?.ClearExpiredTokens();

                // Clean expired sessions
                var sessionService = scope.ServiceProvider.GetService<ClientSessionService>();
                sessionService?.CleanExpiredSessions();

                _logger.LogDebug("Completed cleanup cycle");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cleanup cycle");
            }

            return Task.CompletedTask;
        }
    }
}
