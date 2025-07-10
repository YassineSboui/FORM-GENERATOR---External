using System.Collections.Concurrent;

namespace NeoForm_Externe.Services
{
    public class ClientSessionService
    {
        private readonly ConcurrentDictionary<string, ClientSessionInfo> _sessions = new();
        private readonly ILogger<ClientSessionService> _logger;

        public ClientSessionService(ILogger<ClientSessionService> logger)
        {
            _logger = logger;
        }

        public string GetOrCreateSession(string clientId, string userAgent, string ipAddress)
        {
            var sessionKey = GenerateSessionKey(clientId, userAgent, ipAddress);

            if (_sessions.TryGetValue(sessionKey, out var existingSession))
            {
                existingSession.LastAccessed = DateTime.UtcNow;
                _logger.LogDebug($"Found existing session for client {clientId}: {sessionKey}");
                return sessionKey;
            }

            var newSession = new ClientSessionInfo
            {
                ClientId = clientId,
                SessionId = sessionKey,
                CreatedAt = DateTime.UtcNow,
                LastAccessed = DateTime.UtcNow,
                UserAgent = userAgent,
                IpAddress = ipAddress
            };

            _sessions[sessionKey] = newSession;
            _logger.LogInformation($"Created new session for client {clientId}: {sessionKey}");
            return sessionKey;
        }

        public bool IsValidSession(string sessionKey, string clientId)
        {
            if (!_sessions.TryGetValue(sessionKey, out var session))
            {
                return false;
            }

            if (session.ClientId != clientId)
            {
                _logger.LogWarning($"Session {sessionKey} belongs to client {session.ClientId}, not {clientId}");
                return false;
            }

            if (session.LastAccessed < DateTime.UtcNow.AddHours(-24))
            {
                _sessions.TryRemove(sessionKey, out _);
                _logger.LogInformation($"Removed expired session: {sessionKey}");
                return false;
            }

            return true;
        }

        public void InvalidateSession(string sessionKey)
        {
            if (_sessions.TryRemove(sessionKey, out var session))
            {
                _logger.LogInformation($"Invalidated session for client {session.ClientId}: {sessionKey}");
            }
        }

        public void InvalidateAllSessionsForClient(string clientId)
        {
            var clientSessions = _sessions
                .Where(kvp => kvp.Value.ClientId == clientId)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var sessionKey in clientSessions)
            {
                _sessions.TryRemove(sessionKey, out _);
            }

            _logger.LogInformation($"Invalidated {clientSessions.Count} sessions for client {clientId}");
        }

        public void CleanExpiredSessions()
        {
            var expiredSessions = _sessions
                .Where(kvp => kvp.Value.LastAccessed < DateTime.UtcNow.AddHours(-24))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var sessionKey in expiredSessions)
            {
                _sessions.TryRemove(sessionKey, out _);
            }

            if (expiredSessions.Count > 0)
            {
                _logger.LogInformation($"Cleaned {expiredSessions.Count} expired sessions");
            }
        }

        private string GenerateSessionKey(string clientId, string userAgent, string ipAddress)
        {
            var input = $"{clientId}_{userAgent}_{ipAddress}_{DateTime.UtcNow:yyyyMMdd}";
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(hash)[..16]; // Take first 16 characters
        }

        public ClientSessionInfo? GetSession(string sessionKey)
        {
            _sessions.TryGetValue(sessionKey, out var session);
            return session;
        }
    }

    public class ClientSessionInfo
    {
        public string ClientId { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccessed { get; set; }
        public string UserAgent { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
    }
}
