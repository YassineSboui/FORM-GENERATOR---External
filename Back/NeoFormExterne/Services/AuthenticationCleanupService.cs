using NeoForm_Externe.Interfaces;

namespace NeoForm_Externe.Services
{
    public class AuthenticationCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AuthenticationCleanupService> _logger;

        public AuthenticationCleanupService(IServiceProvider serviceProvider, ILogger<AuthenticationCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var authRepo = scope.ServiceProvider.GetRequiredService<IUserAuthenticationRepository>();
                    await authRepo.CleanupExpiredAuthenticationsAsync();

                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Run every hour
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in authentication cleanup service");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Retry in 5 minutes on error
                }
            }
        }
    }
}
