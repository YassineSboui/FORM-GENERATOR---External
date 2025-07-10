using System.Security.Claims;
using System.Text.Json.Serialization;

namespace NeoForm_Externe.Models
{
    public class OidcModels
    {
        public class OidcValidationResult
        {
            public bool IsValid { get; set; }
            public string Error { get; set; }
            public string UserId { get; set; }
            public string UserEmail { get; set; }
            public IEnumerable<Claim> Claims { get; set; }
            public TokenResponse Tokens { get; set; }
        }

        public class OidcConfiguration
        {
            public string Authority { get; set; }
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
            public string RedirectUri { get; set; }
            public string Scope { get; set; }
        }

        public class TokenResponse
        {
            [JsonPropertyName("access_token")]
            public string AccessToken { get; set; }

            [JsonPropertyName("id_token")]
            public string IdToken { get; set; }

            [JsonPropertyName("refresh_token")]
            public string RefreshToken { get; set; }

            [JsonPropertyName("expires_in")]
            public int ExpiresIn { get; set; }

            [JsonPropertyName("token_type")]
            public string TokenType { get; set; }
        }

        public class AuthTypeResponse
        {
            public bool Valid { get; set; }
            public string AuthType { get; set; }
            public AuthConfig AuthConfig { get; set; }
        }

        public class AuthConfig
        {
            public string ClientId { get; set; }
            public string Authority { get; set; }
            public string RedirectUri { get; set; }
            public string Scope { get; set; }
            public string ClientSecret { get; set; }
        }

        public class OidcDiscoveryDocument
        {
            [JsonPropertyName("issuer")]
            public string Issuer { get; set; }

            [JsonPropertyName("token_endpoint")]
            public string TokenEndpoint { get; set; }

            [JsonPropertyName("jwks_uri")]
            public string JwksUri { get; set; }

            [JsonPropertyName("authorization_endpoint")]
            public string AuthorizationEndpoint { get; set; }

            [JsonPropertyName("userinfo_endpoint")]
            public string UserinfoEndpoint { get; set; }

            [JsonPropertyName("end_session_endpoint")]
            public string EndSessionEndpoint { get; set; }

            [JsonPropertyName("scopes_supported")]
            public string[] ScopesSupported { get; set; }

            [JsonPropertyName("response_types_supported")]
            public string[] ResponseTypesSupported { get; set; }

            [JsonPropertyName("grant_types_supported")]
            public string[] GrantTypesSupported { get; set; }
        }

        public class OidcJsonWebKeySet
        {
            public List<OidcJsonWebKey> Keys { get; set; }
        }

        public class OidcJsonWebKey
        {
            public string Kty { get; set; }
            public string Kid { get; set; }
            public string N { get; set; }
            public string E { get; set; }
            public string Alg { get; set; }
            public string Use { get; set; }
        }

        public class OidcTokenValidationResult
        {
            public bool IsValid { get; set; }
            public string Error { get; set; }
            public IEnumerable<Claim> Claims { get; set; }
        }
    }
}
