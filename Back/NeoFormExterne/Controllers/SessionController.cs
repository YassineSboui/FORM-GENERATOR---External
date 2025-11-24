using Microsoft.AspNetCore.Mvc;
using NeoForm_Externe.Services.Authentication;
using NeoForm_Externe.Services.Client;
using NeoForm_Externe.Interfaces;
using NeoForm_Externe.Data;
using NeoFormExterne.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace NeoForm_Externe.Controllers
{
    [ApiController]
    [Route("neoformexternal/local/[controller]")]
    public class SessionController : ControllerBase
    {
        private readonly ClientSessionService _sessionService;
        private readonly TokenService _tokenService;
        private readonly ILogger<SessionController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IClientStoreService _clientStoreService;
        private readonly ExternalNeoFormContext _context;

        public SessionController(
            ClientSessionService sessionService,
            TokenService tokenService,
            ILogger<SessionController> logger,
            IConfiguration configuration,
            IClientStoreService clientStoreService,
            ExternalNeoFormContext context)
        {
            _sessionService = sessionService;
            _tokenService = tokenService;
            _logger = logger;
            _configuration = configuration;
            _clientStoreService = clientStoreService;
            _context = context;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSessionToken([FromBody] SessionTokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Guid) || string.IsNullOrEmpty(request.Code) || string.IsNullOrEmpty(request.AuthType) || string.IsNullOrEmpty(request.ClientId))
                {
                    return BadRequest(new { success = false, error = "Missing required fields" });
                }

                // Get client's API key to use as JWT secret
                if (!_clientStoreService.TryGetClientApiKey(request.ClientId, out var clientApiKey) || string.IsNullOrEmpty(clientApiKey))
                {
                    _logger.LogError("Client API key not found for clientId: {ClientId}", request.ClientId);
                    return StatusCode(500, new { success = false, error = "Client configuration error" });
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(clientApiKey);

                var claims = new List<Claim>
                {
                    new Claim("guid", request.Guid),
                    new Claim("code", request.Code),
                    new Claim("auth_type", request.AuthType),
                    new Claim("client_id", request.ClientId),
                    new Claim("sub", request.Sub ?? "anonymous"),
                    new Claim(JwtRegisteredClaimNames.Iss, "NeoFormExternal"),
                    new Claim(JwtRegisteredClaimNames.Aud, "NeoFormClient")
                };

                if (!string.IsNullOrEmpty(request.Email))
                {
                    claims.Add(new Claim("email", request.Email));
                }

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(2),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                _logger.LogInformation("Session token created for guid: {Guid}, auth_type: {AuthType}", request.Guid, request.AuthType);

                // 📊 Log session audit
                await LogSessionAudit(new SessionAudit
                {
                    ClientId = request.ClientId,
                    Guid = Guid.Parse(request.Guid),
                    PersonalCode = request.Code,
                    AuthType = request.AuthType,
                    Email = request.Email,
                    TokenId = ((JwtSecurityToken)token).Id,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(2),
                    IpAddress = GetClientIpAddress(),
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                    Success = true
                });

                return Ok(new
                {
                    success = true,
                    sessionToken = tokenString,
                    expiresIn = 7200 // 2 hours in seconds
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating session token");

                // 📊 Log failed session attempt
                await LogSessionAudit(new SessionAudit
                {
                    ClientId = request.ClientId,
                    Guid = Guid.Parse(request.Guid),
                    PersonalCode = request.Code,
                    AuthType = request.AuthType,
                    Email = request.Email,
                    CreatedAt = DateTime.UtcNow,
                    Success = false,
                    ErrorMessage = ex.Message
                });

                return StatusCode(500, new { success = false, error = "Failed to create session token" });
            }
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

        private async Task LogSessionAudit(SessionAudit audit)
        {
            try
            {
                // TODO: SessionAudits DbSet doesn't exist in ExternalNeoFormContext yet
                // Uncomment when SessionAudit DbSet is added to the context
                // _context.SessionAudits.Add(audit);
                // await _context.SaveChangesAsync();
                _logger.LogInformation(
                    "Session audit (not persisted): ClientId={ClientId}, Guid={Guid}, AuthType={AuthType}, Success={Success}",
                    audit.ClientId, audit.Guid, audit.AuthType, audit.Success
                );
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                // Don't fail the request if audit logging fails
                _logger.LogWarning(ex, "Failed to log session audit (non-critical)");
            }
        }

        private string GetClientIpAddress()
        {
            try
            {
                // Check for forwarded IP (if behind proxy)
                var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    return forwardedFor.Split(',')[0].Trim();
                }

                // Get direct connection IP
                return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            }
            catch
            {
                return "unknown";
            }
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

    public class SessionTokenRequest
    {
        public string Guid { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string AuthType { get; set; } = string.Empty; // "otp", "oidc", "none"
        public string ClientId { get; set; } = string.Empty; // client identifier for API key lookup
        public string Email { get; set; } = string.Empty;
        public string Sub { get; set; } = string.Empty; // user identifier
    }
}
