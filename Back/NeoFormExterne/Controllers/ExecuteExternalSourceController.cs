using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NeoForm_Externe.Interfaces;
using NeoForm_Externe.Models;

namespace NeoForm_Externe.Controllers
{
    [ApiController]
    [Route("neoformexternal/local/ExternalSource")]
    [Authorize]
    public class ExecuteExternalSourceController : ControllerBase

    {
        private readonly IExternalSourceService _externalSourceService;
        public ExecuteExternalSourceController(IExternalSourceService externalSourceService)
        {
            _externalSourceService = externalSourceService;
        }

        [HttpPost("ExecuteDbqByObjectName")]
        [ProducesResponseType(typeof(ObjectModels), 200)]
        public async Task<IActionResult> Execute(ExecuteQueryRequest req)
        {
            var o = await _externalSourceService.ExecuteDbqByObjectName(req);
            return Ok(o);
        }

        [HttpPost("ExecuteApiByObjectName")]
        [ProducesResponseType(typeof(ObjectModels), 200)]
        public async Task<IActionResult> ExecuteByObjectName([FromBody] executeApiRequestByObjectName req)
        {
            var o = await _externalSourceService.ExecuteApiByObjectName(req);
            return Ok(o);
        }
    }
}
