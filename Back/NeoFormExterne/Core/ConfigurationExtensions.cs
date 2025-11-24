using NeoForm_Externe.Services.Configuration;

namespace NeoForm_Externe.Core
{
    /// <summary>
    /// Extension methods for IConfiguration to easily get decrypted values
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Gets a decrypted configuration value if it starts with "ENC:", otherwise returns the plain value
        /// </summary>
        public static string GetDecrypted(this IConfiguration configuration, string key, ConfigurationEncryptionService encryptionService)
        {
            return encryptionService.GetDecryptedValue(key);
        }

        /// <summary>
        /// Gets a decrypted connection string
        /// </summary>
        public static string GetDecryptedConnectionString(this IConfiguration configuration, string name, ConfigurationEncryptionService encryptionService)
        {
            return encryptionService.GetDecryptedConnectionString(name);
        }
    }
}
