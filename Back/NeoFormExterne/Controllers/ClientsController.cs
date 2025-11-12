using Microsoft.AspNetCore.Mvc;
using NeoForm_Externe.Interfaces;
using NeoForm_Externe.Models.Dto;
using NeoForm_Externe.Services;
using NeoForm_Externe.Filters;
using Microsoft.AspNetCore.Authorization;

namespace NeoFormExterne.Controllers
{
    [ApiController]
    [Route("neoformexternal/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin")] // Secure entire controller for Admin and SuperAdmin
    public class ClientsController : ControllerBase
    {
        private readonly IClientStoreService _clientStore;
        private readonly ILogger<ClientsController> _logger;

        public ClientsController(IClientStoreService clientStore, ILogger<ClientsController> logger)
        {
            _clientStore = clientStore;
            _logger = logger;
        }

        [HttpGet]
        public ActionResult<Dictionary<string, string>> GetClients()
        {
            var username = User.Identity?.Name ?? "Anonymous";
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var roles = User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();

            _logger.LogInformation("📋 ClientsController.GetClients: Request by '{Username}', Authenticated: {IsAuth}, Roles: {Roles}",
                username, isAuthenticated, string.Join(", ", roles));

            var clients = _clientStore.GetAllClients();
            _logger.LogInformation("✅ ClientsController.GetClients: Returning {Count} clients to '{Username}'", clients.Count, username);

            return Ok(clients);
        }

        [HttpPost]
        public IActionResult AddClient([FromBody] ClientDto clientDto)
        {
            var username = User.Identity?.Name ?? "Anonymous";
            _logger.LogInformation("➕ ClientsController.AddClient: Request to add client '{ClientId}' by '{Username}'",
                clientDto.ClientId, username);

            if (string.IsNullOrEmpty(clientDto.ApiKey))
            {
                _clientStore.AddClient(clientDto.ClientId, clientDto.Url);
                _logger.LogInformation("✅ ClientsController.AddClient: Client '{ClientId}' added without API key", clientDto.ClientId);
            }
            else
            {
                _clientStore.AddClient(clientDto.ClientId, clientDto.Url, clientDto.ApiKey);
                _logger.LogInformation("✅ ClientsController.AddClient: Client '{ClientId}' added with API key", clientDto.ClientId);
            }
            return Ok(new { message = "Client added successfully" });
        }

        [HttpPut("{clientId}")]
        public IActionResult UpdateClient(string clientId, [FromBody] UpdateClientRequest request)
        {
            var username = User.Identity?.Name ?? "Anonymous";
            _logger.LogInformation("✏️ ClientsController.UpdateClient: Request to update client '{ClientId}' by '{Username}'",
                clientId, username);

            if (string.IsNullOrEmpty(request.Url))
            {
                _logger.LogWarning("⚠️ ClientsController.UpdateClient: Empty URL provided for client '{ClientId}'", clientId);
                return BadRequest("New URL cannot be empty");
            }

            if (string.IsNullOrEmpty(request.ApiKey))
            {
                _clientStore.UpdateClient(clientId, request.Url);
                _logger.LogInformation("✅ ClientsController.UpdateClient: Client '{ClientId}' updated without API key", clientId);
            }
            else
            {
                _clientStore.UpdateClient(clientId, request.Url, request.ApiKey);
                _logger.LogInformation("✅ ClientsController.UpdateClient: Client '{ClientId}' updated with API key", clientId);
            }
            return Ok(new { message = "Client updated successfully" });
        }

        [HttpDelete("{clientId}")]
        public IActionResult DeleteClient(string clientId)
        {
            var username = User.Identity?.Name ?? "Anonymous";
            _logger.LogInformation("🗑️ ClientsController.DeleteClient: Request to delete client '{ClientId}' by '{Username}'",
                clientId, username);

            _clientStore.RemoveClient(clientId);
            _logger.LogInformation("✅ ClientsController.DeleteClient: Client '{ClientId}' deleted successfully", clientId);

            return Ok();
        }

        [HttpGet("Clients")]
        [AllowAnonymous] // Keep this endpoint public
        [ServiceFilter(typeof(DynamicApiKeyAuthFilter))]
        public IActionResult GetClientByUrl([FromQuery] string url)
        {
            _logger.LogInformation("🔍 ClientsController.GetClientByUrl: Looking for client with URL '{Url}'", url);

            var client = _clientStore.GetClientByUrl(url);

            if (client == null)
            {
                _logger.LogWarning("⚠️ ClientsController.GetClientByUrl: Client not found for URL '{Url}'", url);
                return NotFound("Client not found");
            }

            _logger.LogInformation("✅ ClientsController.GetClientByUrl: Client found for URL '{Url}'", url);
            return Ok(client);
        }

        [HttpGet("{clientId}/apikey")]
        [AllowAnonymous] // Keep this endpoint public
        public IActionResult GetClientApiKey(string clientId)
        {
            _logger.LogInformation("🔑 ClientsController.GetClientApiKey: Request for API key of client '{ClientId}'", clientId);

            if (_clientStore.TryGetClientApiKey(clientId, out var apiKey))
            {
                _logger.LogInformation("✅ ClientsController.GetClientApiKey: API key found for client '{ClientId}'", clientId);
                return Ok(new { clientId, apiKey });
            }

            _logger.LogWarning("⚠️ ClientsController.GetClientApiKey: Client '{ClientId}' not found", clientId);
            return NotFound("Client not found");
        }
    }

    public class UpdateClientRequest
    {
        public string Url { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }
}
