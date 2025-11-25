using Microsoft.AspNetCore.Mvc;
using NeoForm_Externe.Interfaces;
using NeoForm_Externe.Models;

namespace NeoForm_Externe.Controllers
{
    [ApiController]
    [Route("neoformexternal/local/api/[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly ISessionService _sessionService;
        private readonly ILogger<SessionController> _logger;

        public SessionController(
            ISessionService sessionService,
            ILogger<SessionController> logger)
        {
            _sessionService = sessionService;
            _logger = logger;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSessionToken([FromBody] SessionTokenRequest request)
        {
            var ipAddress = GetClientIpAddress();
            var userAgent = Request.Headers["User-Agent"].ToString();

            var result = await _sessionService.CreateSessionTokenAsync(request, ipAddress, userAgent);

            if (!result.Success)
            {
                if (result.Error == "Client configuration error")
                {
                    return StatusCode(500, new { success = false, error = result.Error });
                }
                return BadRequest(new { success = false, error = result.Error });
            }

            return Ok(new
            {
                success = result.Success,
                sessionToken = result.SessionToken,
                expiresIn = result.ExpiresIn
            });
        }

        [HttpPost("switch-client")]
        public async Task<IActionResult> SwitchClient([FromBody] SwitchClientRequest request)
        {
            var userAgent = Request.Headers.UserAgent.ToString();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var result = await _sessionService.SwitchClientAsync(request, userAgent, ipAddress);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("invalidate-session")]
        public async Task<IActionResult> InvalidateSession([FromBody] InvalidateSessionRequest request)
        {
            try
            {
                await _sessionService.InvalidateSessionAsync(request);
                return Ok(new { Success = true, Message = "Session invalidated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error invalidating session");
                return BadRequest(new { Success = false, Message = "Failed to invalidate session" });
            }
        }

        [HttpGet("session-info/{sessionKey}")]
        public IActionResult GetSessionInfo(string sessionKey)
        {
            var session = _sessionService.GetSessionInfo(sessionKey);
            if (session == null)
            {
                return NotFound(new { Success = false, Message = "Session not found" });
            }

            return Ok(session);
        }

        /// <summary>
        /// Get session audit history for a specific client
        /// </summary>
        [HttpGet("audit-history")]
        public async Task<IActionResult> GetAuditHistory(
            [FromQuery] string? clientId = null,
            [FromQuery] string? guid = null,
            [FromQuery] int pageSize = 50,
            [FromQuery] int page = 1)
        {
            try
            {
                var result = await _sessionService.GetAuditHistoryAsync(clientId, guid, pageSize, page);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving session audit history");
                return StatusCode(500, new { Success = false, Message = "Failed to retrieve audit history" });
            }
        }

        /// <summary>
        /// Get session statistics for a client
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetSessionStats([FromQuery] string? clientId = null, [FromQuery] int days = 7)
        {
            try
            {
                var result = await _sessionService.GetSessionStatsAsync(clientId, days);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving session statistics");
                return StatusCode(500, new { Success = false, Message = "Failed to retrieve statistics" });
            }
        }

        private string GetClientIpAddress()
        {
            try
            {
                var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    return forwardedFor.Split(',')[0].Trim();
                }

                return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            }
            catch
            {
                return "unknown";
            }
        }
    }
}
