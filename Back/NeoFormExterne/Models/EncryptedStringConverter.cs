using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NeoForm_Externe.Interfaces;

namespace NeoForm_Externe.Models
{
    /// <summary>
    /// EF Core Value Converter that automatically encrypts/decrypts values in the database
    /// </summary>
    public class EncryptedStringConverter : ValueConverter<string, string>
    {
        public EncryptedStringConverter(IEncryptionService encryptionService)
            : base(
                // Convert to database: Encrypt the value
                plainText => string.IsNullOrEmpty(plainText)
                    ? plainText
                    : encryptionService.Encrypt(plainText),

                // Convert from database: Decrypt the value
                encryptedText => string.IsNullOrEmpty(encryptedText)
                    ? encryptedText
                    : encryptionService.Decrypt(encryptedText)
            )
        {
        }
    }
}
