using NeoForm_Externe.Interfaces;
using NeoForm_Externe.Models;
using NeoForm_Externe.Data;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI;
using System.Configuration;

namespace NeoForm_Externe.Services
{
    public class ClientStoreService : IClientStoreService
    {
        private readonly ExternalNeoFormContext _context;
        private readonly Dictionary<string, string> _clients = new();
        private readonly IConfiguration _configuration;
        public ClientStoreService(ExternalNeoFormContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            LoadClientsFromDatabase();
        }

        private void LoadClientsFromDatabase()
        {
            var clients = _context.Clients.ToList();
            foreach (var client in clients)
            {
                _clients[client.ClientId] = client.BaseUrl;
            }
        }

        public bool TryGetClient(string clientId, out string url)
        {
            return _clients.TryGetValue(clientId, out url);
        }

        public void AddClient(string clientId, string url)
        {
            if (!_clients.ContainsKey(clientId))
            {
                _context.Clients.Add(new ClientInfo { ClientId = clientId, BaseUrl = url });
                _context.SaveChanges();
                _clients[clientId] = url;
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
            }
        }

        public void RemoveClient(string clientId)
        {
            var client = _context.Clients.FirstOrDefault(c => c.ClientId == clientId);
            if (client != null)
            {
                _context.Clients.Remove(client);
                _context.SaveChanges();
                _clients.Remove(clientId);
            }
        }

        public Dictionary<string, string> GetAllClients()
        {
            return new Dictionary<string, string>(_clients);
        }

        public string? GetClientByUrl(string url)
        {
            var client = _context.Clients.FirstOrDefault(c => c.BaseUrl == url);

            if (client == null) return null;

            var frontUrl = _configuration["FrontFormUrl"]?.TrimEnd('/');
            return $"{frontUrl}/form/{client.ClientId}";
        }
    }
}
