using NeoForm_Externe.Data;
using NeoForm_Externe.Interfaces;

namespace NeoForm_Externe.Services
{
    public class DynamicClientProvider : IDynamicClientProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public DynamicClientProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Dictionary<string, string> GetClients()
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ExternalNeoFormContext>();
            return db.Clients.ToDictionary(c => c.ClientId, c => c.BaseUrl);
        }
    }
}
