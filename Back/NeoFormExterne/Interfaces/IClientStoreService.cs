namespace NeoForm_Externe.Interfaces
{
    public interface IClientStoreService
    {
        bool TryGetClient(string clientId, out string? url);
        Task<bool> TryGetClientAsync(string clientId);
        Task<string?> GetClientUrlAsync(string clientId);
        bool TryGetClientApiKey(string clientId, out string? apiKey);
        void AddClient(string clientId, string url);
        void AddClient(string clientId, string url, string apiKey);
        void UpdateClient(string clientId, string newUrl);
        void UpdateClient(string clientId, string newUrl, string apiKey);
        void RemoveClient(string clientId);
        Dictionary<string, string> GetAllClients();
        string? GetClientByUrl(string url);
    }
}
