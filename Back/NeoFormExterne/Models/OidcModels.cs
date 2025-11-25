using System.Security.Claims;
using System.Text.Json.Serialization;

namespace NeoForm_Externe.Models
{
    // OIDC Configuration
    public class OidcConfiguration
    {
        public string Authority { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
    }

    public class OidcDiscoveryDocument
    {
        [JsonPropertyName("issuer")]
        public string Issuer { get; set; } = string.Empty;

        [JsonPropertyName("token_endpoint")]
        public string TokenEndpoint { get; set; } = string.Empty;

        [JsonPropertyName("jwks_uri")]
        public string JwksUri { get; set; } = string.Empty;

        [JsonPropertyName("authorization_endpoint")]
        public string AuthorizationEndpoint { get; set; } = string.Empty;

        [JsonPropertyName("userinfo_endpoint")]
        public string UserinfoEndpoint { get; set; } = string.Empty;

        [JsonPropertyName("end_session_endpoint")]
        public string EndSessionEndpoint { get; set; } = string.Empty;

        [JsonPropertyName("scopes_supported")]
        public string[] ScopesSupported { get; set; } = Array.Empty<string>();

        [JsonPropertyName("response_types_supported")]
        public string[] ResponseTypesSupported { get; set; } = Array.Empty<string>();

        [JsonPropertyName("grant_types_supported")]
        public string[] GrantTypesSupported { get; set; } = Array.Empty<string>();
    }

    // OIDC Keys
    public class OidcJsonWebKeySet
    {
        public List<OidcJsonWebKey> Keys { get; set; } = new();
    }

    public class OidcJsonWebKey
    {
        public string Kty { get; set; } = string.Empty;
        public string Kid { get; set; } = string.Empty;
        public string N { get; set; } = string.Empty;
        public string E { get; set; } = string.Empty;
        public string Alg { get; set; } = string.Empty;
        public string Use { get; set; } = string.Empty;
    }

    // OIDC Validation Results
    public class OidcValidationResult
    {
        public bool IsValid { get; set; }
        public string Error { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public IEnumerable<Claim> Claims { get; set; } = Enumerable.Empty<Claim>();
        public TokenResponse Tokens { get; set; } = new();
    }

    public class OidcTokenValidationResult
    {
        public bool IsValid { get; set; }
        public string Error { get; set; } = string.Empty;
        public IEnumerable<Claim> Claims { get; set; } = Enumerable.Empty<Claim>();
    }

    // Token Response
    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("id_token")]
        public string IdToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;
    }
}
