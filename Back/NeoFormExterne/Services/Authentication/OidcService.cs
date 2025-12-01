using Microsoft.IdentityModel.Tokens;
using NeoForm_Externe.Interfaces;
using NeoForm_Externe.Models;
using NeoForm_Externe.Models.Dto;
using RestSharp;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Serilog;
using System.Security.Cryptography;
using System.IO;
using System.Text;

namespace NeoForm_Externe.Services.Authentication
{
    public class OidcService : IOidcService
    {
        private readonly ILogger<OidcService> _logger;
        private readonly IUserAuthenticationRepository _authRepository;
        private readonly IEncryptionService _encryption;

        public OidcService(ILogger<OidcService> logger, IUserAuthenticationRepository authRepository, IEncryptionService encryption)
        {
            _logger = logger;
            _authRepository = authRepository;
            _encryption = encryption;
        }

        private RestClient CreateRestClient(string? baseUrl = null)
        {
            var options = new RestClientOptions();
            if (!string.IsNullOrEmpty(baseUrl))
            {
                try
                {
                    options.BaseUrl = new Uri(baseUrl);
                }
                catch
                {
                    // ignore invalid base url here - we'll still create client
                }
            }

            options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
            {
                // Allow local development self-signed certificates for localhost and 127.0.0.1 only
                var url = options.BaseUrl?.ToString() ?? baseUrl ?? string.Empty;
                if (url.StartsWith("https://localhost", StringComparison.OrdinalIgnoreCase) ||
                    url.StartsWith("https://127.0.0.1", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return sslPolicyErrors == SslPolicyErrors.None;
            };

            return new RestClient(options);
        }

        public async Task<OidcValidationResult> ValidateOidcCodeAsync(string code, string state, string guid, string configUrl, string personalCode = "")
        {
            try
            {
                _logger.LogInformation("Starting OIDC validation for GUID: {Guid}, Personal Code: {PersonalCode}", guid, personalCode);
                Log.Information("Starting OIDC validation for GUID: {Guid}, Personal Code: {PersonalCode}", guid, personalCode);

                // 1. Get OIDC configuration
                var oidcConfig = await GetOidcConfigForGuidAsync(configUrl, guid, personalCode);
                if (oidcConfig == null)
                {
                    Log.Warning("Failed to retrieve OIDC configuration for GUID: {Guid}", guid);
                    return new OidcValidationResult
                    {
                        IsValid = false,
                        Error = "Failed to retrieve OIDC configuration"
                    };
                }

                // 2. Exchange code for tokens
                Log.Information("Exchanging authorization code for tokens for GUID: {Guid}", guid);
                var tokens = await ExchangeCodeForTokensAsync(code, oidcConfig);
                if (tokens == null)
                {
                    Log.Warning("Failed to exchange code for tokens for GUID: {Guid}", guid);
                    return new OidcValidationResult
                    {
                        IsValid = false,
                        Error = "Failed to exchange code for tokens"
                    };
                }

                // 3. Validate ID token
                var validationResult = await ValidateIdTokenAsync(tokens.IdToken, oidcConfig);
                if (!validationResult.IsValid)
                {
                    return new OidcValidationResult
                    {
                        IsValid = false,
                        Error = validationResult.Error
                    };
                }

                // 4. Extract user information
                Log.Information("Available claims: {Claims}",
                    string.Join(", ", validationResult.Claims.Select(c => $"{c.Type}:{c.Value}")));

                var userId = validationResult.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
                var userEmail = validationResult.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

                // Try alternative claim types if standard ones are not found
                if (string.IsNullOrEmpty(userId))
                {
                    userId = validationResult.Claims.FirstOrDefault(c =>
                        c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier" ||
                        c.Type == "user_id" || c.Type == "uid")?.Value;
                }

                if (string.IsNullOrEmpty(userEmail))
                {
                    userEmail = validationResult.Claims.FirstOrDefault(c =>
                        c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress" ||
                        c.Type == "user_email")?.Value;
                }

                Log.Information("Extracted - UserId: {UserId}, UserEmail: {UserEmail}", userId, userEmail);
                _logger.LogInformation("OIDC validation successful for GUID: {Guid}, User: {UserId}", guid, userId);
                Log.Information("OIDC validation successful for GUID: {Guid}, User: {UserId}", guid, userId);

                return new OidcValidationResult
                {
                    IsValid = true,
                    UserId = userId,
                    UserEmail = userEmail,
                    Claims = validationResult.Claims,
                    Tokens = tokens
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OIDC validation for GUID: {Guid}", guid);
                return new OidcValidationResult
                {
                    IsValid = false,
                    Error = ex.Message
                };
            }
        }

        public async Task<bool> StoreUserAuthenticationAsync(string guid, string userId, string userEmail, TokenResponse tokens)
        {
            try
            {
                if (tokens == null)
                {
                    _logger.LogError("Tokens are null for GUID: {Guid}, User: {UserId}", guid, userId);
                    return false;
                }

                var userAuth = new UserAuthenticationDto.UserAuthentication
                {
                    Id = Guid.NewGuid().ToString(),
                    Guid = guid,
                    UserId = userId,
                    UserEmail = userEmail,
                    AccessToken = tokens.AccessToken ?? "",
                    IdToken = tokens.IdToken,
                    RefreshToken = tokens.RefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddSeconds(tokens.ExpiresIn),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                // Remove any existing authentication for this GUID
                await _authRepository.RevokeAuthenticationAsync(guid);

                // Store the new authentication
                await _authRepository.StoreAuthenticationAsync(userAuth);

                _logger.LogInformation("Stored authentication for GUID: {Guid}, User: {UserId}", guid, userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing authentication for GUID: {Guid}", guid);
                return false;
            }
        }

        public async Task<bool> IsUserAuthenticatedAsync(string guid)
        {
            try
            {
                var auth = await _authRepository.GetAuthenticationAsync(guid);
                if (auth == null || !auth.IsActive)
                {
                    return false;
                }

                // Check if token is expired
                if (auth.ExpiresAt <= DateTime.UtcNow)
                {
                    _logger.LogInformation("Authentication expired for GUID: {Guid}", guid);
                    await _authRepository.RevokeAuthenticationAsync(guid);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking authentication for GUID: {Guid}", guid);
                return false;
            }
        }

        public async Task RevokeUserAuthenticationAsync(string guid)
        {
            try
            {
                await _authRepository.RevokeAuthenticationAsync(guid);
                _logger.LogInformation("Revoked authentication for GUID: {Guid}", guid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking authentication for GUID: {Guid}", guid);
            }
        }

        private async Task<OidcConfiguration> GetOidcConfigForGuidAsync(string configUrl, string guid, string personalCode)
        {
            try
            {
                var client = CreateRestClient(configUrl);
                var request = new RestRequest(configUrl);
                request.AddQueryParameter("guid", guid);
                request.AddQueryParameter("code", personalCode); // Use personal code instead of OIDC code
                // log the Guid and Personal Code for debugging
                _logger.LogInformation("Retrieving OIDC config for GUID: {Guid}, Personal Code: {PersonalCode}", guid, personalCode);
                Log.Information("Retrieving OIDC config for GUID: {Guid}, Personal Code: {PersonalCode}", guid, personalCode);
                // Log the full request URL for debugging
                var fullUrl = client.BuildUri(request);
                _logger.LogInformation("Requesting OIDC config from URL: {Url}", fullUrl);
                Log.Information("Requesting OIDC config from URL: {Url}", fullUrl);

                var response = await client.ExecuteAsync(request);
                // Log the response status and content
                _logger.LogInformation("Received response for OIDC config: {StatusCode}, Content: {Content}",
                    response.StatusCode, response.Content);
                Log.Information("Received response for OIDC config: {StatusCode}, Content: {Content}",
                    response.StatusCode, response.Content);
                if (!response.IsSuccessful)
                {
                    _logger.LogWarning("Failed to get auth config. Status: {StatusCode}, Content: {Content}", response.StatusCode, response.Content);
                    Log.Warning("Failed to get auth config. Status: {StatusCode}, Content: {Content}", response.StatusCode, response.Content);
                    return null;
                }

                var authResponse = JsonSerializer.Deserialize<AuthTypeResponse>(response.Content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (authResponse?.Valid == true && authResponse.AuthType == "oidc" && authResponse.AuthConfig != null)
                {
                    // Decrypt client secret if provider returned an encrypted value using the specific legacy scheme
                    string clientSecret = authResponse.AuthConfig.ClientSecret ?? string.Empty;
                    try
                    {
                        if (!string.IsNullOrEmpty(clientSecret))
                        {
                            // Try to decrypt with the legacy encryptor used by the other repo
                            // That encryptor uses AES CBC with a 16-char key "YourSecretKey123" and IV "HR$2pIjHR$2pIj12"
                            clientSecret = TryDecryptLegacyClientSecret(clientSecret) ?? clientSecret;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to decrypt client secret for GUID: {Guid}. Using raw value.", guid);
                    }

                    return new OidcConfiguration
                    {
                        Authority = authResponse.AuthConfig.Authority,
                        ClientId = authResponse.AuthConfig.ClientId,
                        ClientSecret = clientSecret,
                        RedirectUri = authResponse.AuthConfig.RedirectUri,
                        Scope = authResponse.AuthConfig.Scope
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving OIDC configuration for GUID: {Guid}", guid);
                return null;
            }
        }

        // Decrypts a cipherText that was encrypted by the other repository's EncryptionService
        // Returns decrypted string or null if decryption failed
        private string? TryDecryptLegacyClientSecret(string cipherText)
        {
            if (string.IsNullOrWhiteSpace(cipherText)) return null;

            try
            {
                // The other service used a 16-byte key: "YourSecretKey123" and IV: "HR$2pIjHR$2pIj12"
                var legacyKey = "YourSecretKey123";
                var legacyIv = "HR$2pIjHR$2pIj12";

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Encoding.UTF8.GetBytes(legacyKey);
                    aesAlg.IV = Encoding.UTF8.GetBytes(legacyIv);

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    var cipherBytes = Convert.FromBase64String(cipherText);
                    using (var msDecrypt = new MemoryStream(cipherBytes))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        var decrypted = srDecrypt.ReadToEnd();
                        // Log masked encrypted and decrypted values for debug (masked to avoid leaking secrets)
                        try
                        {
                            _logger.LogDebug("Legacy decrypt - encrypted(masked)={EncryptedMask}, decrypted(masked)={DecryptedMask}", Mask(cipherText), Mask(decrypted));
                        }
                        catch { }
                        return decrypted;
                    }
                }
            }
            catch
            {
                // Return null to indicate decryption failed; caller will use raw value
                return null;
            }
        }

        private static string Mask(string s)
        {
            if (string.IsNullOrEmpty(s)) return s ?? string.Empty;
            if (s.Length <= 12) return s;
            return s.Substring(0, 6) + "..." + s.Substring(s.Length - 4);
        }

        private async Task<TokenResponse> ExchangeCodeForTokensAsync(string code, OidcConfiguration oidcConfig)
        {
            try
            {
                var tokenEndpoint = await GetTokenEndpointAsync(oidcConfig.Authority);
                Log.Information("Token endpoint: {TokenEndpoint}", tokenEndpoint);

                if (string.IsNullOrEmpty(tokenEndpoint))
                {
                    Log.Error("Token endpoint is null or empty for authority: {Authority}", oidcConfig.Authority);
                    return null;
                }

                var client = CreateRestClient(tokenEndpoint);
                var request = new RestRequest()
                {
                    Method = Method.Post
                };

                request.AddParameter("grant_type", "authorization_code");
                request.AddParameter("code", code);
                request.AddParameter("redirect_uri", oidcConfig.RedirectUri);
                request.AddParameter("client_id", oidcConfig.ClientId);
                request.AddParameter("client_secret", oidcConfig.ClientSecret);

                Log.Information("Exchanging code for tokens at endpoint: {TokenEndpoint}", tokenEndpoint);
                Log.Information("Token request parameters: grant_type=authorization_code, client_id={ClientId}, redirect_uri={RedirectUri}",
                    oidcConfig.ClientId, oidcConfig.RedirectUri);

                var response = await client.ExecuteAsync(request);

                Log.Debug("Token exchange response: Status={StatusCode}, Content={Content}",
                    response.StatusCode, response.Content);

                if (!response.IsSuccessful)
                {
                    _logger.LogWarning("Error exchanging code for tokens: {Error}", response.Content);
                    Log.Warning("Error exchanging code for tokens. Status: {StatusCode}, Content: {Error}", response.StatusCode, response.Content);
                    return null;
                }

                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(response.Content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                Log.Information("Successfully received tokens. Access token length: {AccessTokenLength}, ID token length: {IdTokenLength}",
                    tokenResponse?.AccessToken?.Length ?? 0, tokenResponse?.IdToken?.Length ?? 0);

                return tokenResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exchanging code for tokens");
                Log.Error(ex, "Error exchanging code for tokens");
                return null;
            }
        }

        private async Task<string> GetTokenEndpointAsync(string authority)
        {
            try
            {
                if (!authority.StartsWith("http://") && !authority.StartsWith("https://"))
                {
                    authority = "https://" + authority;
                }

                // Try multiple well-known endpoint variations
                var discoveryUrls = new[]
                {
                    $"{authority}/.well-known/openid-configuration",
                    $"{authority}/.well-known/openid_configuration",
                    $"{authority}/.well-known/oauth-authorization-server",
                    $"{authority}/oauth2/.well-known/openid-configuration" // Some providers use this
                };

                foreach (var configUrl in discoveryUrls)
                {
                    try
                    {
                        Log.Information("Attempting to get token endpoint from: {ConfigUrl}", configUrl);

                        var client = CreateRestClient(configUrl);
                        var request = new RestRequest();

                        var response = await client.ExecuteGetAsync(request);

                        Log.Information("OIDC discovery response from {ConfigUrl}: Status={StatusCode}",
                            configUrl, response.StatusCode);

                        if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                        {
                            var config = JsonSerializer.Deserialize<OidcDiscoveryDocument>(response.Content,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                            if (!string.IsNullOrEmpty(config?.TokenEndpoint))
                            {
                                Log.Information("Successfully found token endpoint: {TokenEndpoint} from {ConfigUrl}",
                                    config.TokenEndpoint, configUrl);
                                return config.TokenEndpoint;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Failed to get discovery document from {ConfigUrl}", configUrl);
                        continue; // Try next URL
                    }
                }

                Log.Warning("Failed to get token endpoint from all well-known configs, using fallback");
                return GetFallbackTokenEndpoint(authority);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token endpoint from authority: {Authority}", authority);
                return GetFallbackTokenEndpoint(authority);
            }
        }

        private string GetFallbackTokenEndpoint(string authority)
        {
            // Auth0
            if (authority.Contains("auth0.com"))
                return $"{authority}/oauth/token";

            // Microsoft Azure AD
            if (authority.Contains("microsoftonline.com"))
                return $"{authority}/oauth2/v2.0/token";

            // Google
            if (authority.Contains("accounts.google.com"))
                return "https://oauth2.googleapis.com/token";

            // Okta
            if (authority.Contains("okta.com") || authority.Contains("oktapreview.com"))
                return $"{authority}/oauth2/v1/token";

            // Keycloak
            if (authority.Contains("keycloak"))
            {
                if (authority.Contains("/auth/realms/"))
                    return $"{authority}/protocol/openid-connect/token";
                return $"{authority}/auth/realms/master/protocol/openid-connect/token";
            }

            // Amazon Cognito
            if (authority.Contains("cognito-idp") && authority.Contains("amazonaws.com"))
                return $"{authority}/oauth2/token";

            // Ping Identity
            if (authority.Contains("pingone.") || authority.Contains("pingidentity."))
                return $"{authority}/as/token.oauth2";

            // OneLogin
            if (authority.Contains("onelogin.com"))
                return $"{authority}/oidc/2/token";

            // Generic fallback - standard OIDC endpoint
            return $"{authority}/oauth2/token";
        }

        private async Task<OidcTokenValidationResult> ValidateIdTokenAsync(string idToken, OidcConfiguration oidcConfig)
        {
            try
            {
                if (!oidcConfig.Authority.StartsWith("http://") && !oidcConfig.Authority.StartsWith("https://"))
                {
                    oidcConfig.Authority = "https://" + oidcConfig.Authority;
                }

                // Try multiple discovery endpoints
                var discoveryUrls = new[]
                {
                    $"{oidcConfig.Authority}/.well-known/openid-configuration",
                    $"{oidcConfig.Authority}/.well-known/openid_configuration",
                    $"{oidcConfig.Authority}/.well-known/oauth-authorization-server"
                };

                OidcDiscoveryDocument config = null;
                foreach (var discoveryUrl in discoveryUrls)
                {
                    try
                    {
                        var client = CreateRestClient(discoveryUrl);
                        var response = await client.ExecuteGetAsync(new RestRequest());

                        if (response.IsSuccessful)
                        {
                            config = JsonSerializer.Deserialize<OidcDiscoveryDocument>(response.Content,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                            if (!string.IsNullOrEmpty(config?.JwksUri))
                            {
                                Log.Information("Using discovery document from: {DiscoveryUrl}", discoveryUrl);
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Failed to get discovery document from {DiscoveryUrl}", discoveryUrl);
                        continue;
                    }
                }

                if (config == null || string.IsNullOrEmpty(config.JwksUri))
                {
                    return new OidcTokenValidationResult { IsValid = false, Error = "Failed to get OIDC configuration" };
                }

                // Get JWKS
                var jwksClient = CreateRestClient(config.JwksUri);
                var jwksResponse = await jwksClient.ExecuteGetAsync(new RestRequest());

                if (!jwksResponse.IsSuccessful)
                {
                    return new OidcTokenValidationResult { IsValid = false, Error = "Failed to get JWKS" };
                }

                var jwks = JsonSerializer.Deserialize<OidcJsonWebKeySet>(jwksResponse.Content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Convert JWKS to signing keys - support multiple key types
                var signingKeys = new List<SecurityKey>();
                foreach (var key in jwks.Keys)
                {
                    try
                    {
                        if (key.Kty == "RSA" && !string.IsNullOrEmpty(key.N) && !string.IsNullOrEmpty(key.E))
                        {
                            var rsaKey = new RsaSecurityKey(
                                new System.Security.Cryptography.RSAParameters
                                {
                                    Modulus = Base64UrlEncoder.DecodeBytes(key.N),
                                    Exponent = Base64UrlEncoder.DecodeBytes(key.E)
                                })
                            {
                                KeyId = key.Kid
                            };
                            signingKeys.Add(rsaKey);
                        }
                        // Could add support for EC keys here if needed
                        // else if (key.Kty == "EC") { ... }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Failed to process key {KeyId} of type {KeyType}", key.Kid, key.Kty);
                        continue;
                    }
                }

                if (!signingKeys.Any())
                {
                    return new OidcTokenValidationResult { IsValid = false, Error = "No valid signing keys found" };
                }

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = config.Issuer,
                    ValidateAudience = true,
                    ValidAudience = oidcConfig.ClientId,
                    ValidateLifetime = true,
                    IssuerSigningKeys = signingKeys,
                    ClockSkew = TimeSpan.FromMinutes(5) // Allow 5 minutes clock skew
                };

                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(idToken, validationParameters, out var validatedToken);

                Log.Information("ID token validated successfully for issuer: {Issuer}", config.Issuer);

                return new OidcTokenValidationResult
                {
                    IsValid = true,
                    Claims = principal.Claims
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating ID token");
                Log.Error(ex, "Error validating ID token");
                return new OidcTokenValidationResult { IsValid = false, Error = ex.Message };
            }
        }
    }
}
