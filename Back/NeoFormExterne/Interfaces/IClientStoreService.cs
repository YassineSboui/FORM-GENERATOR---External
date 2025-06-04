namespace NeoForm_Externe.Interfaces
{
    public interface IClientStoreService
    {
        bool TryGetClient(string clientId, out string url);
        void AddClient(string clientId, string url);
        void UpdateClient(string clientId, string newUrl);
        void RemoveClient(string clientId);
        Dictionary<string, string> GetAllClients();
        string? GetClientByUrl(string url);
    }
}
