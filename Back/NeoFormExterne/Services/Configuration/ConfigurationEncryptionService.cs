using NeoForm_Externe.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NeoForm_Externe.Services.Configuration
{
    /// <summary>
    /// Service that automatically encrypts sensitive configuration values on application startup
    /// if they are not already encrypted.
    /// </summary>
    public class ConfigurationEncryptionService
    {
        private readonly IConfiguration _configuration;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<ConfigurationEncryptionService> _logger;
        private readonly IWebHostEnvironment _environment;

        public ConfigurationEncryptionService(
            IConfiguration configuration,
            IEncryptionService encryptionService,
            ILogger<ConfigurationEncryptionService> logger,
            IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _encryptionService = encryptionService;
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// Encrypts sensitive values in appsettings.json if they are not already encrypted
        /// </summary>
        public void EncryptSensitiveValuesIfNeeded()
        {
            try
            {
                var appSettingsPath = Path.Combine(_environment.ContentRootPath, "appsettings.json");

                if (!File.Exists(appSettingsPath))
                {
                    _logger.LogWarning("appsettings.json not found at {Path}", appSettingsPath);
                    return;
                }

                _logger.LogInformation("Checking for unencrypted sensitive values in appsettings.json...");

                // Read the JSON file
                var jsonContent = File.ReadAllText(appSettingsPath);
                var jsonObject = JObject.Parse(jsonContent);
                bool hasChanges = false;

                // Encrypt ConnectionStrings - User Id and Password
                if (jsonObject["ConnectionStrings"]?["ExternalNeoFormContext"] != null)
                {
                    var connectionString = jsonObject["ConnectionStrings"]!["ExternalNeoFormContext"]!.ToString();
                    var (newConnectionString, changed) = EncryptConnectionString(connectionString);

                    if (changed)
                    {
                        jsonObject["ConnectionStrings"]!["ExternalNeoFormContext"] = newConnectionString;
                        hasChanges = true;
                        _logger.LogInformation("✅ Encrypted database credentials in connection string");
                    }
                }

                // Encrypt Email Username
                if (jsonObject["Email"]?["Username"] != null)
                {
                    var username = jsonObject["Email"]!["Username"]!.ToString();
                    if (!username.StartsWith("ENC:", StringComparison.OrdinalIgnoreCase))
                    {
                        var encrypted = _encryptionService.Encrypt(username);
                        jsonObject["Email"]!["Username"] = $"ENC:{encrypted}";
                        hasChanges = true;
                        _logger.LogInformation("✅ Encrypted Email Username");
                    }
                }

                // Encrypt Email Password
                if (jsonObject["Email"]?["Password"] != null)
                {
                    var password = jsonObject["Email"]!["Password"]!.ToString();
                    if (!password.StartsWith("ENC:", StringComparison.OrdinalIgnoreCase))
                    {
                        var encrypted = _encryptionService.Encrypt(password);
                        jsonObject["Email"]!["Password"] = $"ENC:{encrypted}";
                        hasChanges = true;
                        _logger.LogInformation("✅ Encrypted Email Password");
                    }
                }

                // Encrypt NeoForm X-api-key
                if (jsonObject["NeoForm"]?["X-api-key"] != null)
                {
                    var apiKey = jsonObject["NeoForm"]!["X-api-key"]!.ToString();
                    if (!apiKey.StartsWith("ENC:", StringComparison.OrdinalIgnoreCase))
                    {
                        var encrypted = _encryptionService.Encrypt(apiKey);
                        jsonObject["NeoForm"]!["X-api-key"] = $"ENC:{encrypted}";
                        hasChanges = true;
                        _logger.LogInformation("✅ Encrypted NeoForm X-api-key");
                    }
                }

                // Save changes if any
                if (hasChanges)
                {
                    // Create backup first
                    var backupPath = appSettingsPath + ".backup";
                    File.Copy(appSettingsPath, backupPath, true);
                    _logger.LogInformation("📁 Created backup at {BackupPath}", backupPath);

                    // Write updated JSON with proper formatting
                    var formattedJson = jsonObject.ToString(Formatting.Indented);
                    File.WriteAllText(appSettingsPath, formattedJson);

                    _logger.LogWarning("🔒 Sensitive values have been encrypted in appsettings.json");
                    _logger.LogWarning("⚠️  Please restart the application for changes to take effect");
                    _logger.LogInformation("📋 A backup was created at: {BackupPath}", backupPath);
                }
                else
                {
                    _logger.LogInformation("✅ All sensitive values are already encrypted");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error while encrypting sensitive configuration values");
            }
        }

        /// <summary>
        /// Encrypts User Id and Password in a connection string if not already encrypted
        /// </summary>
        private (string connectionString, bool changed) EncryptConnectionString(string connectionString)
        {
            var parts = connectionString.Split(';');
            bool changed = false;

            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i].Trim();

                // Encrypt User Id
                if (part.StartsWith("User Id=", StringComparison.OrdinalIgnoreCase))
                {
                    var value = part.Substring(part.IndexOf('=') + 1).Trim();
                    if (!value.StartsWith("ENC:", StringComparison.OrdinalIgnoreCase))
                    {
                        var encrypted = _encryptionService.Encrypt(value);
                        parts[i] = $"User Id=ENC:{encrypted}";
                        changed = true;
                    }
                }

                // Encrypt Password
                if (part.StartsWith("Password=", StringComparison.OrdinalIgnoreCase))
                {
                    var value = part.Substring(part.IndexOf('=') + 1).Trim();
                    if (!value.StartsWith("ENC:", StringComparison.OrdinalIgnoreCase))
                    {
                        var encrypted = _encryptionService.Encrypt(value);
                        parts[i] = $"Password=ENC:{encrypted}";
                        changed = true;
                    }
                }
            }

            return (string.Join(";", parts), changed);
        }

        /// <summary>
        /// Gets a decrypted value from configuration
        /// </summary>
        public string GetDecryptedValue(string key)
        {
            var value = _configuration[key];
            if (string.IsNullOrEmpty(value))
                return value ?? string.Empty;

            if (value.StartsWith("ENC:", StringComparison.OrdinalIgnoreCase))
            {
                var encryptedValue = value.Substring(4);
                return _encryptionService.Decrypt(encryptedValue);
            }

            return value;
        }

        /// <summary>
        /// Gets a decrypted connection string
        /// </summary>
        public string GetDecryptedConnectionString(string name)
        {
            var connectionString = _configuration.GetConnectionString(name);
            if (string.IsNullOrEmpty(connectionString))
                return connectionString ?? string.Empty;

            if (connectionString.Contains("ENC:", StringComparison.OrdinalIgnoreCase))
            {
                var parts = connectionString.Split(';');
                for (int i = 0; i < parts.Length; i++)
                {
                    var part = parts[i].Trim();

                    if (part.StartsWith("Password=ENC:", StringComparison.OrdinalIgnoreCase))
                    {
                        var encryptedPassword = part.Substring(part.IndexOf("ENC:") + 4);
                        var decryptedPassword = _encryptionService.Decrypt(encryptedPassword);
                        parts[i] = $"Password={decryptedPassword}";
                    }
                    else if (part.StartsWith("User Id=ENC:", StringComparison.OrdinalIgnoreCase))
                    {
                        var encryptedUserId = part.Substring(part.IndexOf("ENC:") + 4);
                        var decryptedUserId = _encryptionService.Decrypt(encryptedUserId);
                        parts[i] = $"User Id={decryptedUserId}";
                    }
                }
                return string.Join(";", parts);
            }

            return connectionString;
        }
    }
}
