using Microsoft.AspNetCore.Mvc;
using NeoForm_Externe.Interfaces;
using NeoForm_Externe.Models.Dto;
using NeoForm_Externe.Services;
using NeoForm_Externe.Filters;

namespace NeoFormExterne.Controllers
{
    [ApiController]
    [Route("neoformexternal/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly IClientStoreService _clientStore;

        public ClientsController(IClientStoreService clientStore)
        {
            _clientStore = clientStore;
        }

        [HttpGet]
        public ActionResult<Dictionary<string, string>> GetClients()
        {
            return Ok(_clientStore.GetAllClients());
        }

        [HttpPost]
        public IActionResult AddClient([FromBody] ClientDto clientDto)
        {
            if (string.IsNullOrEmpty(clientDto.ApiKey))
            {
                _clientStore.AddClient(clientDto.ClientId, clientDto.Url);
            }
            else
            {
                _clientStore.AddClient(clientDto.ClientId, clientDto.Url, clientDto.ApiKey);
            }
            return Ok(new { message = "Client added successfully" });
        }

        [HttpPut("{clientId}")]
        public IActionResult UpdateClient(string clientId, [FromBody] UpdateClientRequest request)
        {
            if (string.IsNullOrEmpty(request.Url))
            {
                return BadRequest("New URL cannot be empty");
            }

            if (string.IsNullOrEmpty(request.ApiKey))
            {
                _clientStore.UpdateClient(clientId, request.Url);
            }
            else
            {
                _clientStore.UpdateClient(clientId, request.Url, request.ApiKey);
            }
            return Ok(new { message = "Client updated successfully" });
        }

        [HttpDelete("{clientId}")]
        public IActionResult DeleteClient(string clientId)
        {
            _clientStore.RemoveClient(clientId);
            return Ok();
        }

        [HttpGet("Clients")]
        [ServiceFilter(typeof(DynamicApiKeyAuthFilter))]
        public IActionResult GetClientByUrl([FromQuery] string url)
        {
            var client = _clientStore.GetClientByUrl(url);
            return client == null ? NotFound("Client not found") : Ok(client);
        }

        [HttpGet("{clientId}/apikey")]
        public IActionResult GetClientApiKey(string clientId)
        {
            if (_clientStore.TryGetClientApiKey(clientId, out var apiKey))
            {
                return Ok(new { clientId, apiKey });
            }
            return NotFound("Client not found");
        }
    }

    public class UpdateClientRequest
    {
        public string Url { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }
}
