namespace NeoForm_Externe.Models
{
    // Authentication Configuration
    public class AuthConfig
    {
        public bool RequireAuth { get; set; }
        public string ClientId { get; set; } = string.Empty;
        public string Authority { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;

        // Email Auth specific
        public string[] Emails { get; set; } = Array.Empty<string>();
        public bool VerifyEmail { get; set; }
    }

    // Authentication Responses
    public class AuthResponse
    {
        public string Message { get; set; } = string.Empty;
        public string Jwt { get; set; } = string.Empty;
    }

    public class AuthTypeResponse
    {
        public bool RequireAuth { get; set; }
        public string AuthType { get; set; } = string.Empty;
        public AuthConfig AuthConfig { get; set; } = new();
        public bool Valid { get; set; }
    }

    // Client Authentication Info
    public class ClientAuthInfo
    {
        public string Code { get; set; } = string.Empty;
        public string Guid { get; set; } = string.Empty;
        public string TokenApiUrl { get; set; } = string.Empty;
    }

    // Authorization
    public class Authorization
    {
        public string ApiType = string.Empty;
        public string? ApiKey;
        public string? Token;
        public string? key;
        public string? value;
        public string? bearerToken;
        public string? username;
        public string? password;
        public string? algorithm;
        public object? payload;
        public string? secret;
    }

    // Token Info
    public class TokenInfo
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
    }
}
