using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using NeoForm_Externe.Data;

namespace NeoForm_Externe.Services.Background
{
    public class OtpCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OtpCleanupService> _logger;
        private readonly IConfiguration _configuration;
        private TimeSpan _interval;
        private int _retentionHours;
        private bool _cleanupSessionAudits;
        private int _sessionAuditRetentionDays;

        public OtpCleanupService(
            IServiceProvider serviceProvider,
            ILogger<OtpCleanupService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;

            // Read configuration with defaults
            _interval = TimeSpan.FromHours(_configuration.GetValue<int>("OtpCleanup:IntervalHours", 24));
            _retentionHours = _configuration.GetValue<int>("OtpCleanup:RetentionHours", 24);
            _cleanupSessionAudits = _configuration.GetValue<bool>("OtpCleanup:CleanupSessionAudits", false);
            _sessionAuditRetentionDays = _configuration.GetValue<int>("OtpCleanup:SessionAuditRetentionDays", 90);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "OTP Cleanup Service started. Running every {Hours} hours, retaining OTP records for {Retention} hours. " +
                "Session audit cleanup: {SessionAuditEnabled} (retention: {SessionRetention} days)",
                _interval.TotalHours,
                _retentionHours,
                _cleanupSessionAudits ? "Enabled" : "Disabled",
                _sessionAuditRetentionDays
            );

            // Wait 5 minutes after startup before first cleanup
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredOtps(stoppingToken);

                    // Cleanup session audits if enabled
                    if (_cleanupSessionAudits)
                    {
                        await CleanupSessionAudits(stoppingToken);
                    }

                    _logger.LogInformation("Next OTP cleanup scheduled in {Hours} hours", _interval.TotalHours);
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("OTP Cleanup Service is stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in OTP cleanup service");
                    // Retry after 5 minutes on error
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("OTP Cleanup Service stopped");
        }

        private async Task CleanupExpiredOtps(CancellationToken stoppingToken)
        {
            // TODO: OtpCodes table doesn't exist in database yet
            // Uncomment when OtpCode model and DbSet are added to ExternalNeoFormContext
            _logger.LogInformation("OTP cleanup skipped - OtpCodes table not configured");
            await Task.CompletedTask;

            /*
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ExternalNeoFormContext>();

            var cutoffDate = DateTime.UtcNow.AddHours(-_retentionHours);
            var now = DateTime.UtcNow;

            _logger.LogInformation("Starting OTP cleanup. Cutoff date: {CutoffDate}", cutoffDate);

            try
            {
                // Count records before cleanup
                var totalBefore = await context.OtpCodes.CountAsync(stoppingToken);

                // Delete expired codes
                var expiredCodes = await context.OtpCodes
                    .Where(o => o.ExpiresAt < now)
                    .ToListAsync(stoppingToken);

                // Delete used codes
                var usedCodes = await context.OtpCodes
                    .Where(o => o.IsUsed && o.CreatedAt < cutoffDate)
                    .ToListAsync(stoppingToken);

                // Delete old unused codes
                var oldCodes = await context.OtpCodes
                    .Where(o => !o.IsUsed && o.CreatedAt < cutoffDate)
                    .ToListAsync(stoppingToken);

                var totalToDelete = expiredCodes.Count + usedCodes.Count + oldCodes.Count;

                if (totalToDelete > 0)
                {
                    context.OtpCodes.RemoveRange(expiredCodes);
                    context.OtpCodes.RemoveRange(usedCodes);
                    context.OtpCodes.RemoveRange(oldCodes);

                    await context.SaveChangesAsync(stoppingToken);

                    var totalAfter = await context.OtpCodes.CountAsync(stoppingToken);

                    _logger.LogInformation(
                        "OTP Cleanup completed: {Expired} expired, {Used} used, {Old} old codes deleted. " +
                        "Total before: {Before}, Total after: {After}",
                        expiredCodes.Count,
                        usedCodes.Count,
                        oldCodes.Count,
                        totalBefore,
                        totalAfter
                    );

                    // Log to cleanup audit table if it exists
                    await LogCleanupStats(context, totalToDelete, stoppingToken);
                }
                else
                {
                    _logger.LogInformation("No OTP codes to clean up. Total records: {Total}", totalBefore);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OTP cleanup operation");
                throw;
            }
            */
        }

        private async Task LogCleanupStats(ExternalNeoFormContext context, int recordsDeleted, CancellationToken stoppingToken)
        {
            try
            {
                // Check if CleanupLog table exists and log the cleanup
                // This is optional - implement if you have a CleanupLog table
                var cleanupLog = new
                {
                    TableName = "OtpCodes",
                    RecordsDeleted = recordsDeleted,
                    CleanupDate = DateTime.UtcNow
                };

                _logger.LogInformation(
                    "Cleanup stats: {TableName} - {RecordsDeleted} records deleted at {CleanupDate}",
                    cleanupLog.TableName,
                    cleanupLog.RecordsDeleted,
                    cleanupLog.CleanupDate
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to log cleanup stats (this is non-critical)");
            }
        }

        private async Task CleanupSessionAudits(CancellationToken stoppingToken)
        {
            // TODO: SessionAudits table doesn't exist in ExternalNeoFormContext yet
            // Uncomment when SessionAudit DbSet is added to the context
            _logger.LogInformation("Session audit cleanup skipped - SessionAudits table not configured");
            await Task.CompletedTask;

            /*
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ExternalNeoFormContext>();

            var cutoffDate = DateTime.UtcNow.AddDays(-_sessionAuditRetentionDays);

            _logger.LogInformation(
                "Starting Session Audit cleanup. Cutoff date: {CutoffDate} (keeping {Days} days)",
                cutoffDate,
                _sessionAuditRetentionDays
            );

            try
            {
                // Check if SessionAudits table exists in your DbContext
                // Adjust the table/entity name based on your actual schema
                var oldAudits = await context.Set<SessionAudit>()
                    .Where(s => s.CreatedAt < cutoffDate)
                    .ToListAsync(stoppingToken);

                if (oldAudits.Any())
                {
                    context.Set<SessionAudit>().RemoveRange(oldAudits);
                    await context.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation(
                        "Session Audit cleanup completed: {Count} old audit records deleted",
                        oldAudits.Count
                    );
                }
                else
                {
                    _logger.LogInformation("No session audit records to clean up");
                }
            }
            catch (InvalidOperationException ex)
            {
                // Table doesn't exist - log warning and continue
                _logger.LogWarning(
                    "SessionAudit table not found in DbContext. " +
                    "Set CleanupSessionAudits to false or create the SessionAudit entity. Error: {Error}",
                    ex.Message
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Session Audit cleanup operation");
                // Don't throw - let OTP cleanup continue
            }
            */
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OTP Cleanup Service is stopping...");
            await base.StopAsync(stoppingToken);
        }
    }
}
