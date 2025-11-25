using System.Security.Claims;

namespace NeoForm_Externe.Models
{
    public class EmailValidationResult
    {
        public bool IsValid { get; set; }
        public string? Error { get; set; }
    }

    public class OTPSendResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public int ExpiresInSeconds { get; set; } = 300;
    }

    public class OTPVerificationResult
    {
        public bool IsValid { get; set; }
        public string? Error { get; set; }
        public IEnumerable<Claim>? Claims { get; set; }
    }

    public class SessionTokenValidationResult
    {
        public bool IsValid { get; set; }
        public string? Error { get; set; }
        public string Guid { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string AuthType { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Sub { get; set; } = string.Empty;
    }

    public class SessionTokenRequest
    {
        public string Guid { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string AuthType { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Sub { get; set; } = string.Empty;
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

    public class SessionTokenResponse
    {
        public bool Success { get; set; }
        public string? SessionToken { get; set; }
        public int ExpiresIn { get; set; }
        public string? Error { get; set; }
    }

    public class SessionAuditHistory
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<SessionAuditDto> Audits { get; set; } = new();
    }

    public class SessionAuditDto
    {
        public int Id { get; set; }
        public string ClientId { get; set; } = string.Empty;
        public Guid Guid { get; set; }
        public string PersonalCode { get; set; } = string.Empty;
        public string AuthType { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? OidcUserId { get; set; }
        public string? TokenId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class SessionStats
    {
        public string Period { get; set; } = string.Empty;
        public int TotalSessions { get; set; }
        public int SuccessfulSessions { get; set; }
        public int FailedSessions { get; set; }
        public double SuccessRate { get; set; }
        public List<AuthTypeStats> ByAuthType { get; set; } = new();
        public List<ClientStats> TopClients { get; set; } = new();
    }

    public class AuthTypeStats
    {
        public string AuthType { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ClientStats
    {
        public string ClientId { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
