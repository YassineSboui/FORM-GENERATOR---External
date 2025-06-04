using NeoForm_Externe.Models;
using System.Text.Json;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace NeoForm_Externe.Services
{
    public class TokenService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TokenService> _logger;

        private readonly Dictionary<string, TokenInfoModels> _tokens = new();

        public TokenService(IHttpClientFactory httpClientFactory, ILogger<TokenService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<string> GetOrRefreshTokenAsync(string clientId, string baseUrl, string code, string guid)
        {
            var tokenKey = $"{clientId}_{code}_{guid}";

            if (_tokens.TryGetValue(tokenKey, out var tokenInfo))
            {
                if (tokenInfo.Expiration > DateTime.UtcNow)
                {
                    _logger.LogInformation($"✅ Using cached token for client {clientId}");
                    return tokenInfo.Token;
                }
                else
                {
                    _logger.LogInformation($"🔁 Token expired for client {clientId}, refreshing...");
                }
            }
            else
            {
                _logger.LogInformation($"🔓 No token found for client {clientId}, authenticating...");
            }

            var client = _httpClientFactory.CreateClient();

            // Ensure single slash between base URL and API path
            var baseUri = baseUrl.TrimEnd('/');
            var authUrl = $"{baseUri}/api/external/auth?code={code}&guid={guid}";

            var response = await client.GetAsync(authUrl);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"❌ Auth failed ({response.StatusCode}): {errorContent}");
                throw new Exception($"Auth request failed with status {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();

            try
            {
                var payload = JsonSerializer.Deserialize<AuthResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (payload == null || string.IsNullOrWhiteSpace(payload.Jwt))
                {
                    _logger.LogError("❌ Invalid response payload, no JWT found.");
                    throw new Exception("Invalid authentication response: missing token");
                }

                var newToken = payload.Jwt;

                _tokens[tokenKey] = new TokenInfoModels
                {
                    Token = newToken,
                    Expiration = DateTime.UtcNow.AddMinutes(4)
                };

                _logger.LogInformation($"✅ Token refreshed successfully for client {clientId}");
                return newToken;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, $"❌ Failed to parse auth response JSON: {content}");
                throw new Exception("Invalid authentication response format");
            }
        }
    }
}
