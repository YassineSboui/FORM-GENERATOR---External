using NeoForm_Externe.Models;
using System.Text.Json;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;

namespace NeoForm_Externe.Services
{
    public class TokenService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TokenService> _logger;

        // Use ConcurrentDictionary for thread safety
        private readonly ConcurrentDictionary<string, TokenInfoModels> _tokens = new();
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public TokenService(IHttpClientFactory httpClientFactory, ILogger<TokenService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<string> GetOrRefreshTokenAsync(string clientId, string baseUrl, string code, string guid)
        {
            var tokenKey = $"{clientId}_{code}_{guid}";

            // First check if we have a valid cached token
            if (_tokens.TryGetValue(tokenKey, out var tokenInfo) && IsTokenValid(tokenInfo))
            {
                _logger.LogInformation($"✅ Using cached valid token for client {clientId}");
                return tokenInfo.Token;
            }

            // Use semaphore to prevent multiple concurrent token requests for the same key
            await _semaphore.WaitAsync();
            try
            {
                // Double-check after acquiring the semaphore
                if (_tokens.TryGetValue(tokenKey, out tokenInfo) && IsTokenValid(tokenInfo))
                {
                    _logger.LogInformation($"✅ Using cached valid token for client {clientId} (acquired after wait)");
                    return tokenInfo.Token;
                }

                return await RefreshTokenAsync(clientId, baseUrl, code, guid, tokenKey);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private bool IsTokenValid(TokenInfoModels tokenInfo)
        {
            if (tokenInfo.Expiration <= DateTime.UtcNow.AddMinutes(1)) // Add 1 minute buffer
            {
                return false;
            }

            // Additional JWT validation if possible
            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(tokenInfo.Token))
                {
                    var jwtToken = handler.ReadJwtToken(tokenInfo.Token);
                    var exp = jwtToken.Claims.FirstOrDefault(x => x.Type == "exp")?.Value;
                    if (exp != null && long.TryParse(exp, out var expUnix))
                    {
                        var expDateTime = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
                        return expDateTime > DateTime.UtcNow.AddMinutes(1);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to validate JWT token, using fallback expiration");
            }

            return true;
        }

        private async Task<string> RefreshTokenAsync(string clientId, string baseUrl, string code, string guid, string tokenKey)
        {
            _logger.LogInformation($"🔁 Refreshing token for client {clientId}...");

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(30); // Add timeout

            var baseUri = baseUrl.TrimEnd('/');

            // Remove /neoform path if present to get the actual base URL
            if (baseUri.EndsWith("/neoform", StringComparison.OrdinalIgnoreCase))
            {
                baseUri = baseUri.Substring(0, baseUri.Length - "/neoform".Length);
            }

            _logger.LogDebug("📍 Original baseUrl: '{OriginalUrl}', Extracted baseUri: '{ExtractedUri}' for client '{ClientId}'",
                baseUrl, baseUri, clientId);

            var authUrl = $"{baseUri}/api/external/auth?code={code}&guid={guid}";

            _logger.LogDebug("🔗 Auth URL: '{AuthUrl}' for client '{ClientId}'", authUrl, clientId);

            try
            {
                var response = await client.GetAsync(authUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"❌ Auth failed for client {clientId} ({response.StatusCode}): {errorContent}");

                    // Remove invalid token from cache
                    _tokens.TryRemove(tokenKey, out _);

                    throw new UnauthorizedAccessException($"Authentication failed for client {clientId}: {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var payload = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (payload == null || string.IsNullOrWhiteSpace(payload.Jwt))
                {
                    _logger.LogError("❌ Invalid response payload, no JWT found.");
                    throw new UnauthorizedAccessException("Invalid authentication response: missing token");
                }

                var newToken = payload.Jwt;
                var expiration = GetTokenExpiration(newToken);

                _tokens[tokenKey] = new TokenInfoModels
                {
                    Token = newToken,
                    Expiration = expiration
                };

                _logger.LogInformation($"✅ Token refreshed successfully for client {clientId}, expires at {expiration}");
                return newToken;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"❌ Network error while authenticating client {clientId}");
                throw new UnauthorizedAccessException($"Network error during authentication for client {clientId}", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, $"❌ Timeout while authenticating client {clientId}");
                throw new UnauthorizedAccessException($"Authentication timeout for client {clientId}", ex);
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, $"❌ Failed to parse auth response JSON for client {clientId}");
                throw new UnauthorizedAccessException("Invalid authentication response format", jsonEx);
            }
        }

        private DateTime GetTokenExpiration(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(token))
                {
                    var jwtToken = handler.ReadJwtToken(token);
                    var exp = jwtToken.Claims.FirstOrDefault(x => x.Type == "exp")?.Value;
                    if (exp != null && long.TryParse(exp, out var expUnix))
                    {
                        return DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract expiration from JWT, using default");
            }

            // Fallback to 4 minutes
            return DateTime.UtcNow.AddMinutes(4);
        }

        public void InvalidateToken(string clientId, string code, string guid)
        {
            var tokenKey = $"{clientId}_{code}_{guid}";
            _tokens.TryRemove(tokenKey, out _);
            _logger.LogInformation($"🗑️ Token invalidated for client {clientId}");
        }

        public void ClearExpiredTokens()
        {
            var expiredKeys = _tokens
                .Where(kvp => kvp.Value.Expiration <= DateTime.UtcNow)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _tokens.TryRemove(key, out _);
            }

            if (expiredKeys.Count > 0)
            {
                _logger.LogInformation($"🧹 Cleared {expiredKeys.Count} expired tokens");
            }
        }
    }
}
