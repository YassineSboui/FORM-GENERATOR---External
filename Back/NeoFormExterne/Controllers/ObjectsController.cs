using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeoForm_Externe.Attributes;
using NeoForm_Externe.Interfaces;
using NeoForm_Externe.Models;
using Newtonsoft.Json.Linq;


namespace NeoForm_Externe.Controllers
{
    [ApiController]
    [Route("neoformexternal/local/objects")]
    public class ObjectsController : ControllerBase
    {
        private readonly IObjectService _objectService;
        private readonly ILogger<ObjectsController> _logger;
        public ObjectsController(IObjectService objectService, ILogger<ObjectsController> logger)
        {
            _objectService = objectService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetObjectsByType([FromQuery] string objectType)
        {
            if (string.IsNullOrWhiteSpace(objectType))
                return BadRequest("Missing objectType");

            var objects = await _objectService.GetObjectsByTypeAsync(objectType);
            return Ok(objects);
        }

   
        [HttpGet("guid/{guid}")]
        public async Task<IActionResult> GetObjectByGuid(string guid)
        {
            var obj = await _objectService.GetObjectByGuidAsync(guid);
            return obj == null ? NotFound("Object not found") : Ok(obj);
        }

        [HttpPost("Publish")]
        [ApiKeyAuth]
        [ProducesResponseType(typeof(ObjectModels), 200)]
        public async Task<IActionResult> PublishObject([FromBody] JObject obj)
        {
            try
            {
                var result = await _objectService.PublishObject(obj);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish object");
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = $"Internal error: {ex.Message}"
                });
            }
        }

    }
}
