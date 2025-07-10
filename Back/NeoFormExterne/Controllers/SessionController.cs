using Microsoft.AspNetCore.Mvc;
using NeoForm_Externe.Services;

namespace NeoForm_Externe.Controllers
{
    [ApiController]
    [Route("neoformexternal/local/[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly ClientSessionService _sessionService;
        private readonly TokenService _tokenService;
        private readonly ILogger<SessionController> _logger;

        public SessionController(
            ClientSessionService sessionService,
            TokenService tokenService,
            ILogger<SessionController> logger)
        {
            _sessionService = sessionService;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost("switch-client")]
        public IActionResult SwitchClient([FromBody] SwitchClientRequest request)
        {
            try
            {
                // Invalidate current session if provided
                if (!string.IsNullOrEmpty(request.CurrentSessionKey))
                {
                    _sessionService.InvalidateSession(request.CurrentSessionKey);
                }

                // Invalidate tokens for the old client
                if (!string.IsNullOrEmpty(request.CurrentClientId) &&
                    !string.IsNullOrEmpty(request.Code) &&
                    !string.IsNullOrEmpty(request.Guid))
                {
                    _tokenService.InvalidateToken(request.CurrentClientId, request.Code, request.Guid);
                }

                // Create new session for the target client
                var userAgent = Request.Headers.UserAgent.ToString();
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var newSessionKey = _sessionService.GetOrCreateSession(request.TargetClientId, userAgent, ipAddress);

                _logger.LogInformation($"Client switched from {request.CurrentClientId} to {request.TargetClientId}");

                return Ok(new SwitchClientResponse
                {
                    NewSessionKey = newSessionKey,
                    Success = true,
                    Message = "Client switched successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error switching client");
                return BadRequest(new SwitchClientResponse
                {
                    Success = false,
                    Message = "Failed to switch client"
                });
            }
        }

        [HttpPost("invalidate-session")]
        public IActionResult InvalidateSession([FromBody] InvalidateSessionRequest request)
        {
            try
            {
                _sessionService.InvalidateSession(request.SessionKey);

                if (!string.IsNullOrEmpty(request.ClientId) &&
                    !string.IsNullOrEmpty(request.Code) &&
                    !string.IsNullOrEmpty(request.Guid))
                {
                    _tokenService.InvalidateToken(request.ClientId, request.Code, request.Guid);
                }

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
            var session = _sessionService.GetSession(sessionKey);
            if (session == null)
            {
                return NotFound(new { Success = false, Message = "Session not found" });
            }

            return Ok(new SessionInfoResponse
            {
                ClientId = session.ClientId,
                SessionId = session.SessionId,
                CreatedAt = session.CreatedAt,
                LastAccessed = session.LastAccessed,
                IsValid = _sessionService.IsValidSession(sessionKey, session.ClientId)
            });
        }
    }

    public class SwitchClientRequest
    {
        public string CurrentClientId { get; set; } = string.Empty;
        public string TargetClientId { get; set; } = string.Empty;
        public string CurrentSessionKey { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Guid { get; set; } = string.Empty;
    }

    public class SwitchClientResponse
    {
        public string NewSessionKey { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class InvalidateSessionRequest
    {
        public string SessionKey { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Guid { get; set; } = string.Empty;
    }

    public class SessionInfoResponse
    {
        public string ClientId { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccessed { get; set; }
        public bool IsValid { get; set; }
    }
}
