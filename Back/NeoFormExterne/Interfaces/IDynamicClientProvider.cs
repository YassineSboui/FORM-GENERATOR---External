namespace NeoForm_Externe.Interfaces
{
    public interface IDynamicClientProvider
    {
        Dictionary<string, string> GetClients(); // clientId => baseUrl
    }

}
