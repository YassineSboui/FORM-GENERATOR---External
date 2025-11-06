using Microsoft.EntityFrameworkCore;
using NeoForm_Externe.Data;
using NeoForm_Externe.Interfaces;

namespace NeoForm_Externe.Services
{
    /// <summary>
    /// Service to migrate existing plain-text API keys to encrypted format
    /// </summary>
    public class ApiKeyMigrationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<ApiKeyMigrationService> _logger;

        public ApiKeyMigrationService(
            IServiceProvider serviceProvider,
            IEncryptionService encryptionService,
            ILogger<ApiKeyMigrationService> logger)
        {
            _serviceProvider = serviceProvider;
            _encryptionService = encryptionService;
            _logger = logger;
        }

        /// <summary>
        /// Encrypts all existing plain-text API keys in the database
        /// This should be run once during application startup
        /// </summary>
        public async Task MigrateApiKeysAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();

                // Create a DbContext WITHOUT the encryption converter to read plain-text values
                var optionsBuilder = new DbContextOptionsBuilder<ExternalNeoFormContext>();
                var connectionString = scope.ServiceProvider.GetRequiredService<IConfiguration>()
                    .GetConnectionString("ExternalNeoFormContext");

                optionsBuilder.UseSqlServer(connectionString);

                // Use a plain context without encryption to read raw values
                using var plainContext = new ExternalNeoFormContext(optionsBuilder.Options);

                // Get all clients with their raw (possibly unencrypted) API keys
                var clients = await plainContext.Clients.ToListAsync();

                int encryptedCount = 0;

                foreach (var client in clients)
                {
                    if (!string.IsNullOrEmpty(client.ApiKey))
                    {
                        // Try to decrypt - if it fails, it means the value is not encrypted yet
                        bool isAlreadyEncrypted = false;
                        try
                        {
                            var decrypted = _encryptionService.Decrypt(client.ApiKey);
                            isAlreadyEncrypted = true;
                            _logger.LogDebug("Client {ClientId} API key is already encrypted", client.ClientId);
                        }
                        catch
                        {
                            // Not encrypted, needs migration
                            isAlreadyEncrypted = false;
                        }

                        if (!isAlreadyEncrypted)
                        {
                            // Encrypt the plain-text API key
                            var encryptedApiKey = _encryptionService.Encrypt(client.ApiKey);

                            // Update directly in database using raw SQL to bypass the converter
                            await plainContext.Database.ExecuteSqlRawAsync(
                                "UPDATE [AppNeoForm].[Clients] SET [ApiKey] = {0} WHERE [Id] = {1}",
                                encryptedApiKey, client.Id);

                            encryptedCount++;
                            _logger.LogInformation("✅ Encrypted API key for client: {ClientId}", client.ClientId);
                        }
                    }
                }

                if (encryptedCount > 0)
                {
                    _logger.LogWarning("🔒 Encrypted {Count} API keys in the database", encryptedCount);
                }
                else
                {
                    _logger.LogInformation("✅ All API keys are already encrypted");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during API key migration");
            }
        }
    }
}
