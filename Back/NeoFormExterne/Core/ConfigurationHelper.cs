using NeoForm_Externe.Interfaces;

namespace NeoForm_Externe.Core
{
    /// <summary>
    /// Helper class to automatically decrypt configuration values that are prefixed with "ENC:"
    /// </summary>
    public class ConfigurationHelper
    {
        private readonly IConfiguration _configuration;
        private readonly IEncryptionService _encryptionService;

        public ConfigurationHelper(IConfiguration configuration, IEncryptionService encryptionService)
        {
            _configuration = configuration;
            _encryptionService = encryptionService;
        }

        /// <summary>
        /// Gets a configuration value and decrypts it if it starts with "ENC:"
        /// </summary>
        public string GetDecryptedValue(string key)
        {
            var value = _configuration[key];
            if (string.IsNullOrEmpty(value))
                return value ?? string.Empty;

            // If value starts with ENC:, decrypt it
            if (value.StartsWith("ENC:", StringComparison.OrdinalIgnoreCase))
            {
                var encryptedValue = value.Substring(4); // Remove "ENC:" prefix
                return _encryptionService.Decrypt(encryptedValue);
            }

            return value;
        }

        /// <summary>
        /// Gets a connection string and decrypts password if it's encrypted
        /// </summary>
        public string GetDecryptedConnectionString(string name)
        {
            var connectionString = _configuration.GetConnectionString(name);
            if (string.IsNullOrEmpty(connectionString))
                return connectionString ?? string.Empty;

            // Decrypt password in connection string if it's encrypted
            if (connectionString.Contains("Password=ENC:", StringComparison.OrdinalIgnoreCase))
            {
                var parts = connectionString.Split(';');
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i].TrimStart().StartsWith("Password=ENC:", StringComparison.OrdinalIgnoreCase))
                    {
                        var encryptedPassword = parts[i].Substring(parts[i].IndexOf("ENC:") + 4);
                        var decryptedPassword = _encryptionService.Decrypt(encryptedPassword);
                        parts[i] = parts[i].Replace("ENC:" + encryptedPassword, decryptedPassword);
                    }
                    else if (parts[i].TrimStart().StartsWith("User Id=ENC:", StringComparison.OrdinalIgnoreCase))
                    {
                        var encryptedUserId = parts[i].Substring(parts[i].IndexOf("ENC:") + 4);
                        var decryptedUserId = _encryptionService.Decrypt(encryptedUserId);
                        parts[i] = parts[i].Replace("ENC:" + encryptedUserId, decryptedUserId);
                    }
                }
                return string.Join(";", parts);
            }

            return connectionString;
        }
    }
}
