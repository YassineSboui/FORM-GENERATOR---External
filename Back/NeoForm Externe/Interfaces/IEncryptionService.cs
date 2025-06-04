namespace NeoForm_Externe.Interfaces
{
    public interface IEncryptionService
    {
        string Decrypt(string input);
        string Encrypt(string input); // if needed

    }
}
