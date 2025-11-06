using NeoForm_Externe.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace NeoForm_Externe.Services
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
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = Encoding.UTF8.GetBytes(iv);

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        public string Decrypt(string cipherText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(key);
                aesAlg.IV = Encoding.UTF8.GetBytes(iv);

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
