using NeoForm_Externe.Interfaces;
using NeoForm_Externe.Services.Configuration;

namespace NeoForm_Externe.Tools
{
    /// <summary>
    /// Command-line tool to encrypt sensitive configuration values.
    /// Usage: dotnet run --project NeoFormExterne encrypt "your-value-here"
    /// </summary>
    public class EncryptionTool
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("=== Configuration Value Encryption Tool ===");
                Console.WriteLine();
                Console.WriteLine("This tool helps you encrypt sensitive values for appsettings.json");
                Console.WriteLine();
                Console.WriteLine("Usage:");
                Console.WriteLine("  dotnet run --project NeoFormExterne encrypt <value>");
                Console.WriteLine();
                Console.WriteLine("Example:");
                Console.WriteLine("  dotnet run --project NeoFormExterne encrypt \"mypassword123\"");
                Console.WriteLine();
                Console.WriteLine("Enter a value to encrypt (or press Enter to exit):");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    return;
                }

                EncryptValue(input);
            }
            else if (args[0].Equals("encrypt", StringComparison.OrdinalIgnoreCase) && args.Length > 1)
            {
                EncryptValue(args[1]);
            }
            else
            {
                Console.WriteLine("Invalid arguments. Use: encrypt <value>");
            }
        }

        private static void EncryptValue(string value)
        {
            IEncryptionService encryptionService = new EncryptionService();
            var encrypted = encryptionService.Encrypt(value);

            Console.WriteLine();
            Console.WriteLine("Original value:");
            Console.WriteLine($"  {value}");
            Console.WriteLine();
            Console.WriteLine("Encrypted value (use this in appsettings.json with ENC: prefix):");
            Console.WriteLine($"  ENC:{encrypted}");
            Console.WriteLine();
            Console.WriteLine("Full configuration format:");
            Console.WriteLine($"  \"YourKey\": \"ENC:{encrypted}\"");
            Console.WriteLine();

            // Test decryption
            var decrypted = encryptionService.Decrypt(encrypted);
            Console.WriteLine($"Verification (decrypted): {decrypted}");
            Console.WriteLine($"Match: {value == decrypted}");
        }
    }
}
