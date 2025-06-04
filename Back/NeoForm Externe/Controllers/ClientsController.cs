using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeoForm_Externe.Attributes;
using NeoForm_Externe.Interfaces;
using NeoForm_Externe.Models.Dto;
using NeoForm_Externe.Services;

namespace NeoFormExterne.Controllers
{
    [ApiController]
    [Route("neoformexternal/[controller]")]
    [Authorize(Roles = "Admin")]
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
            _clientStore.AddClient(clientDto.ClientId, clientDto.Url);
            return Ok();
        }

        [HttpPut("{clientId}")]
        public IActionResult UpdateClient(string clientId, [FromBody] string newUrl)
        {
            if (string.IsNullOrEmpty(newUrl))
            {
                return BadRequest("New URL cannot be empty");
            }

            _clientStore.UpdateClient(clientId, newUrl);
            return Ok();
        }

        [HttpDelete("{clientId}")]
        public IActionResult DeleteClient(string clientId)
        {
            _clientStore.RemoveClient(clientId);
            return Ok();
        }

        [HttpGet("Clients")]
        [AllowAnonymous]
        [ApiKeyAuth]
        public IActionResult GetClientByUrl([FromQuery] string url)
        {
            var client = _clientStore.GetClientByUrl(url);
            return client == null ? NotFound("Client not found") : Ok(client);
        }
    }
}
