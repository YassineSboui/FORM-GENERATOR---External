using NeoForm_Externe.Interfaces;
using NeoForm_Externe.Models;
using NeoForm_Externe.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace NeoForm_Externe.Services
{
    public class ClientStoreService : IClientStoreService
    {
        private readonly ExternalNeoFormContext _context;
        private readonly ConcurrentDictionary<string, string> _clients = new();
        private readonly ConcurrentDictionary<string, string> _clientApiKeys = new();
        private readonly IConfiguration _configuration;
        private readonly ILogger<ClientStoreService> _logger;
        private readonly SemaphoreSlim _reloadSemaphore = new(1, 1);
        private DateTime _lastReload = DateTime.UtcNow;
        private readonly TimeSpan _reloadInterval = TimeSpan.FromMinutes(5);

        public ClientStoreService(ExternalNeoFormContext context, IConfiguration configuration, ILogger<ClientStoreService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            LoadClientsFromDatabase();
        }

        private void LoadClientsFromDatabase()
        {
            try
            {
                var clients = _context.Clients.ToList();
                _clients.Clear();
                _clientApiKeys.Clear();
                foreach (var client in clients)
                {
                    _clients[client.ClientId] = client.BaseUrl;
                    _clientApiKeys[client.ClientId] = client.ApiKey;
                }
                _lastReload = DateTime.UtcNow;
                _logger.LogInformation($"Loaded {clients.Count} clients from database");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load clients from database");
            }
        }

        public async Task<bool> TryGetClientAsync(string clientId)
        {
            // Check if we need to reload
            if (DateTime.UtcNow - _lastReload > _reloadInterval)
            {
                await _reloadSemaphore.WaitAsync();
                try
                {
                    if (DateTime.UtcNow - _lastReload > _reloadInterval)
                    {
                        LoadClientsFromDatabase();
                    }
                }
                finally
                {
                    _reloadSemaphore.Release();
                }
            }

            return _clients.ContainsKey(clientId);
        }

        public async Task<string?> GetClientUrlAsync(string clientId)
        {
            if (await TryGetClientAsync(clientId))
            {
                _clients.TryGetValue(clientId, out var url);
                return url;
            }
            return null;
        }

        public bool TryGetClient(string clientId, out string? url)
        {
            return _clients.TryGetValue(clientId, out url);
        }

        public bool TryGetClientApiKey(string clientId, out string? apiKey)
        {
            return _clientApiKeys.TryGetValue(clientId, out apiKey);
        }

        public void AddClient(string clientId, string url)
        {
            AddClient(clientId, url, GenerateApiKey());
        }

        public void AddClient(string clientId, string url, string apiKey)
        {
            if (!_clients.ContainsKey(clientId))
            {
                _context.Clients.Add(new ClientInfo { ClientId = clientId, BaseUrl = url, ApiKey = apiKey });
                _context.SaveChanges();
                _clients[clientId] = url;
                _clientApiKeys[clientId] = apiKey;
                _logger.LogInformation($"Added new client: {clientId} -> {url} with API key");
            }
        }

        public void UpdateClient(string clientId, string newUrl)
        {
            var client = _context.Clients.FirstOrDefault(c => c.ClientId == clientId);
            if (client != null)
            {
                client.BaseUrl = newUrl;
                _context.SaveChanges();
                _clients[clientId] = newUrl;
                _logger.LogInformation($"Updated client: {clientId} -> {newUrl}");
            }
        }

        public void UpdateClient(string clientId, string newUrl, string apiKey)
        {
            var client = _context.Clients.FirstOrDefault(c => c.ClientId == clientId);
            if (client != null)
            {
                client.BaseUrl = newUrl;
                client.ApiKey = apiKey;
                _context.SaveChanges();
                _clients[clientId] = newUrl;
                _clientApiKeys[clientId] = apiKey;
                _logger.LogInformation($"Updated client: {clientId} -> {newUrl} with new API key");
            }
        }

        public void RemoveClient(string clientId)
        {
            var client = _context.Clients.FirstOrDefault(c => c.ClientId == clientId);
            if (client != null)
            {
                _context.Clients.Remove(client);
                _context.SaveChanges();
                _clients.TryRemove(clientId, out _);
                _logger.LogInformation($"Removed client: {clientId}");
            }
        }

        public Dictionary<string, string> GetAllClients()
        {
            return new Dictionary<string, string>(_clients);
        }

        public string? GetClientByUrl(string url)
        {
            _logger.LogDebug("🔍 Looking for client with URL: '{Url}'", url);

            var client = _context.Clients.FirstOrDefault(c => c.BaseUrl == (url + "/neoform"));

            if (client == null)
            {
                _logger.LogWarning("❌ No client found with exact URL match: '{Url}'", url);

                // Log all existing client URLs for debugging
                var allClients = _context.Clients.ToList();
                _logger.LogDebug("📋 Available client URLs in database:");
                foreach (var c in allClients)
                {
                    _logger.LogDebug("   - Client '{ClientId}': '{BaseUrl}'", c.ClientId, c.BaseUrl);
                }

                return null;
            }

            _logger.LogInformation("✅ Found client '{ClientId}' for URL '{Url}'", client.ClientId, url);

            var frontUrl = _configuration["FrontFormUrl"]?.TrimEnd('/');
            var result = $"{frontUrl}/form/{client.ClientId}";

            _logger.LogDebug("🔗 Returning form URL: '{FormUrl}'", result);

            return result;
        }

        private string GenerateApiKey()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
