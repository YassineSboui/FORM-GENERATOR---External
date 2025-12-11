using NeoForm_Externe.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace NeoForm_Externe.Services.Configuration
{
    public class EncryptionService : IEncryptionService
    {
        // Use environment variables or keep these default keys
        // For production, set ENCRYPTION_KEY and ENCRYPTION_IV environment variables
        private readonly string key;
        private readonly string iv;

        public EncryptionService()
        {
            // Try to get encryption key from environment variable, otherwise use default
            key = Environment.GetEnvironmentVariable("ENCRYPTION_KEY") ?? "YourSecretKey123456789012345"; // Must be 32 chars for AES-256
            iv = Environment.GetEnvironmentVariable("ENCRYPTION_IV") ?? "HR$2pIjHR$2pIj12"; // Must be 16 chars for AES

            // Ensure proper key length (32 bytes for AES-256)
            if (key.Length < 32)
            {
                key = key.PadRight(32, '0');
            }
            else if (key.Length > 32)
            {
                key = key.Substring(0, 32);
            }

            // Ensure proper IV length (16 bytes)
            if (iv.Length < 16)
            {
                iv = iv.PadRight(16, '0');
            }
            else if (iv.Length > 16)
            {
                iv = iv.Substring(0, 16);
            }
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText ?? string.Empty;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = Encoding.UTF8.GetBytes(iv);

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                    swEncrypt.Flush();
                    csEncrypt.FlushFinalBlock();
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        public string Decrypt(string cipherText)
        {
            // Handle null/empty safely
            if (string.IsNullOrWhiteSpace(cipherText))
                return cipherText ?? string.Empty;

            cipherText = cipherText.Trim();

            // If it's clearly not Base64, treat it as already-plain text
            if (!IsBase64(cipherText))
            {
                // Optional: you can add logging here if you inject an ILogger
                // e.g. _logger.LogWarning("Decrypt called with non-Base64 value. Returning raw text.");
                return cipherText;
            }

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Encoding.UTF8.GetBytes(key);
                    aesAlg.IV = Encoding.UTF8.GetBytes(iv);

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
            catch (FormatException)
            {
                // If somehow still not valid Base64, fall back to original
                return cipherText;
            }
            catch (CryptographicException)
            {
                // If key/IV mismatch or corrupt data, also fall back
                return cipherText;
            }
        }

        private static bool IsBase64(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            // Quick safe check that doesn't throw
            Span<byte> buffer = stackalloc byte[input.Length];
            return Convert.TryFromBase64String(input, buffer, out _);
        }
    }
}
