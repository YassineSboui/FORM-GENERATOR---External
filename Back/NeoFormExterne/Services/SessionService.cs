using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NeoForm_Externe.Data;
using NeoForm_Externe.Interfaces;
using NeoForm_Externe.Models;
using NeoForm_Externe.Services.Authentication;
using NeoForm_Externe.Services.Client;
using NeoFormExterne.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NeoForm_Externe.Services
{
    public class SessionService : ISessionService
    {
        private readonly ClientSessionService _clientSessionService;
        private readonly TokenService _tokenService;
        private readonly ILogger<SessionService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IClientStoreService _clientStoreService;
        private readonly ExternalNeoFormContext _context;

        public SessionService(
            ClientSessionService clientSessionService,
            TokenService tokenService,
            ILogger<SessionService> logger,
            IConfiguration configuration,
            IClientStoreService clientStoreService,
            ExternalNeoFormContext context)
        {
            _clientSessionService = clientSessionService;
            _tokenService = tokenService;
            _logger = logger;
            _configuration = configuration;
            _clientStoreService = clientStoreService;
            _context = context;
        }

        public async Task<SessionTokenResponse> CreateSessionTokenAsync(SessionTokenRequest request, string ipAddress, string userAgent)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Guid) || string.IsNullOrEmpty(request.Code) ||
                    string.IsNullOrEmpty(request.AuthType) || string.IsNullOrEmpty(request.ClientId))
                {
                    return new SessionTokenResponse
                    {
                        Success = false,
                        Error = "Missing required fields"
                    };
                }

                // Get client's API key to use as JWT secret
                if (!_clientStoreService.TryGetClientApiKey(request.ClientId, out var clientApiKey) || string.IsNullOrEmpty(clientApiKey))
                {
                    _logger.LogError("Client API key not found for clientId: {ClientId}", request.ClientId);
                    return new SessionTokenResponse
                    {
                        Success = false,
                        Error = "Client configuration error"
                    };
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

                // Log session audit
                await LogSessionAuditAsync(new SessionAudit
                {
                    ClientId = request.ClientId,
                    Guid = Guid.Parse(request.Guid),
                    PersonalCode = request.Code,
                    AuthType = request.AuthType,
                    Email = request.Email,
                    TokenId = ((JwtSecurityToken)token).Id,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(2),
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Success = true
                });

                return new SessionTokenResponse
                {
                    Success = true,
                    SessionToken = tokenString,
                    ExpiresIn = 7200 // 2 hours in seconds
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating session token");

                // Log failed session attempt
                await LogSessionAuditAsync(new SessionAudit
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

                return new SessionTokenResponse
                {
                    Success = false,
                    Error = "Failed to create session token"
                };
            }
        }

        public async Task<SwitchClientResponse> SwitchClientAsync(SwitchClientRequest request, string userAgent, string ipAddress)
        {
            try
            {
                // Invalidate current session if provided
                if (!string.IsNullOrEmpty(request.CurrentSessionKey))
                {
                    _clientSessionService.InvalidateSession(request.CurrentSessionKey);
                }

                // Invalidate tokens for the old client
                if (!string.IsNullOrEmpty(request.CurrentClientId) &&
                    !string.IsNullOrEmpty(request.Code) &&
                    !string.IsNullOrEmpty(request.Guid))
                {
                    _tokenService.InvalidateToken(request.CurrentClientId, request.Code, request.Guid);
                }

                // Create new session for the target client
                var newSessionKey = _clientSessionService.GetOrCreateSession(request.TargetClientId, userAgent, ipAddress);

                _logger.LogInformation($"Client switched from {request.CurrentClientId} to {request.TargetClientId}");

                return await Task.FromResult(new SwitchClientResponse
                {
                    NewSessionKey = newSessionKey,
                    Success = true,
                    Message = "Client switched successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error switching client");
                return await Task.FromResult(new SwitchClientResponse
                {
                    Success = false,
                    Message = "Failed to switch client"
                });
            }
        }

        public async Task InvalidateSessionAsync(InvalidateSessionRequest request)
        {
            _clientSessionService.InvalidateSession(request.SessionKey);

            if (!string.IsNullOrEmpty(request.ClientId) &&
                !string.IsNullOrEmpty(request.Code) &&
                !string.IsNullOrEmpty(request.Guid))
            {
                _tokenService.InvalidateToken(request.ClientId, request.Code, request.Guid);
            }

            await Task.CompletedTask;
        }

        public SessionInfoResponse? GetSessionInfo(string sessionKey)
        {
            var session = _clientSessionService.GetSession(sessionKey);
            if (session == null)
            {
                return null;
            }

            return new SessionInfoResponse
            {
                ClientId = session.ClientId,
                SessionId = session.SessionId,
                CreatedAt = session.CreatedAt,
                LastAccessed = session.LastAccessed,
                IsValid = _clientSessionService.IsValidSession(sessionKey, session.ClientId)
            };
        }

        public async Task<SessionAuditHistory> GetAuditHistoryAsync(string? clientId, string? guid, int pageSize, int page)
        {
            var query = _context.SessionAudits.AsQueryable();

            if (!string.IsNullOrEmpty(clientId))
            {
                query = query.Where(a => a.ClientId == clientId);
            }

            if (!string.IsNullOrEmpty(guid))
            {
                var guidParsed = Guid.Parse(guid);
                query = query.Where(a => a.Guid == guidParsed);
            }

            var total = await query.CountAsync();
            var audits = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new SessionAuditDto
                {
                    Id = a.Id,
                    ClientId = a.ClientId,
                    Guid = a.Guid,
                    PersonalCode = a.PersonalCode,
                    AuthType = a.AuthType,
                    Email = a.Email,
                    OidcUserId = a.OidcUserId,
                    TokenId = a.TokenId,
                    CreatedAt = a.CreatedAt,
                    ExpiresAt = a.ExpiresAt,
                    IpAddress = a.IpAddress,
                    UserAgent = a.UserAgent,
                    Success = a.Success,
                    ErrorMessage = a.ErrorMessage
                })
                .ToListAsync();

            return new SessionAuditHistory
            {
                Total = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                Audits = audits
            };
        }

        public async Task<SessionStats> GetSessionStatsAsync(string? clientId, int days)
        {
            var since = DateTime.UtcNow.AddDays(-days);
            var query = _context.SessionAudits.Where(a => a.CreatedAt >= since);

            if (!string.IsNullOrEmpty(clientId))
            {
                query = query.Where(a => a.ClientId == clientId);
            }

            var totalSessions = await query.CountAsync();
            var successfulSessions = await query.Where(a => a.Success).CountAsync();
            var failedSessions = totalSessions - successfulSessions;

            var byAuthType = await query
                .GroupBy(a => a.AuthType)
                .Select(g => new AuthTypeStats { AuthType = g.Key, Count = g.Count() })
                .ToListAsync();

            var byClient = await query
                .GroupBy(a => a.ClientId)
                .Select(g => new ClientStats { ClientId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            return new SessionStats
            {
                Period = $"Last {days} days",
                TotalSessions = totalSessions,
                SuccessfulSessions = successfulSessions,
                FailedSessions = failedSessions,
                SuccessRate = totalSessions > 0 ? (successfulSessions * 100.0 / totalSessions) : 0,
                ByAuthType = byAuthType,
                TopClients = byClient
            };
        }

        private async Task LogSessionAuditAsync(SessionAudit audit)
        {
            try
            {
                _context.SessionAudits.Add(audit);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Session audit persisted: ClientId={ClientId}, Guid={Guid}, AuthType={AuthType}, Success={Success}",
                    audit.ClientId, audit.Guid, audit.AuthType, audit.Success
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to log session audit (non-critical)");
            }
        }
    }
}
