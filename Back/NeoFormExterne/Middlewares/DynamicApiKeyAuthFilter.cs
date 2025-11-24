using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NeoForm_Externe.Interfaces;

namespace NeoForm_Externe.Middlewares
{
    public class DynamicApiKeyAuthFilter : IAuthorizationFilter
    {
        private const string HeaderName = "X-api-key";
        private readonly IClientStoreService _clientStoreService;
        private readonly ILogger<DynamicApiKeyAuthFilter> _logger;

        public DynamicApiKeyAuthFilter(IClientStoreService clientStoreService, ILogger<DynamicApiKeyAuthFilter> logger)
        {
            _clientStoreService = clientStoreService;
            _logger = logger;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            _logger.LogDebug("DynamicApiKeyAuthFilter triggered for path: {Path}, Method: {Method}",
                context.HttpContext.Request.Path, context.HttpContext.Request.Method);

            // Get the API key from request header
            if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var extractedKey))
            {
                _logger.LogWarning("Missing X-api-key header in request from {RemoteIpAddress} to {Path}",
                    context.HttpContext.Connection.RemoteIpAddress,
                    context.HttpContext.Request.Path);
                context.Result = new UnauthorizedObjectResult(new { error = "Missing X-api-key header" });
                return;
            }

            // Get the URL from query parameters
            var urlFromQuery = context.HttpContext.Request.Query["url"].FirstOrDefault();

            _logger.LogDebug("Request params - URL: '{UrlFromQuery}'", urlFromQuery);

            if (string.IsNullOrEmpty(urlFromQuery))
            {
                _logger.LogWarning("Missing 'url' query parameter in request from {RemoteIpAddress} to {Path}",
                    context.HttpContext.Connection.RemoteIpAddress,
                    context.HttpContext.Request.Path);
                context.Result = new UnauthorizedObjectResult(new { error = "Missing 'url' query parameter" });
                return;
            }

            // Validate the API key against client database using the URL from query params
            var apiKeyValue = extractedKey.ToString();
            if (string.IsNullOrEmpty(apiKeyValue) || !ValidateApiKeyWithUrl(apiKeyValue, urlFromQuery))
            {
                _logger.LogWarning("Invalid API key '{ApiKey}' for URL '{Url}' from {RemoteIpAddress} to {Path}",
                    apiKeyValue, urlFromQuery, context.HttpContext.Connection.RemoteIpAddress,
                    context.HttpContext.Request.Path);
                context.Result = new UnauthorizedObjectResult(new { error = "Invalid API key for this URL" });
                return;
            }

            _logger.LogInformation("API key validated successfully for URL '{Url}' to {Path}", urlFromQuery, context.HttpContext.Request.Path);
        }

        private bool ValidateApiKeyWithUrl(string apiKey, string clientUrl)
        {
            try
            {
                // Get all clients from database
                var clients = _clientStoreService.GetAllClients();

                _logger.LogDebug("🔍 Validating request: URL='{Url}', API Key='{ApiKey}', Total Clients={ClientCount}",
                    clientUrl, apiKey, clients.Count);

                // Look through all clients to find one whose URL matches the request URL
                foreach (var client in clients)
                {
                    var storedClientUrl = client.Value; // This is the full URL like "https://example.com/neoform"

                    _logger.LogDebug("🔍 Comparing request URL '{RequestUrl}' with client '{ClientId}' URL '{ClientUrl}'",
                        clientUrl, client.Key, storedClientUrl);

                    // Check if the request URL matches this client's URL (ignoring /neoform path)
                    if (IsUrlMatching(clientUrl, storedClientUrl))
                    {
                        _logger.LogInformation("🎯 URL MATCH: Found client '{ClientId}' for URL '{Url}'",
                            client.Key, clientUrl);

                        // Now check if the API key matches this client's API key
                        if (_clientStoreService.TryGetClientApiKey(client.Key, out var clientApiKey))
                        {
                            _logger.LogDebug("🔑 Checking API key for client '{ClientId}': stored='{StoredKey}' vs received='{ReceivedKey}'",
                                client.Key, clientApiKey, apiKey);

                            if (clientApiKey == apiKey)
                            {
                                _logger.LogInformation("✅ SUCCESS: Both URL and API key match for client '{ClientId}'", client.Key);
                                return true;
                            }
                            else
                            {
                                _logger.LogWarning("❌ API KEY MISMATCH: Client '{ClientId}' found by URL but API key doesn't match. Expected='{Expected}', Got='{Received}'",
                                    client.Key, clientApiKey, apiKey);
                                return false; // Found the right client but wrong API key
                            }
                        }
                        else
                        {
                            _logger.LogWarning("❌ Client '{ClientId}' matches URL but has no API key configured", client.Key);
                            return false;
                        }
                    }
                    else
                    {
                        _logger.LogDebug("❌ URL mismatch: '{RequestUrl}' ≠ '{ClientUrl}' (client '{ClientId}')",
                            clientUrl, storedClientUrl, client.Key);
                    }
                }

                _logger.LogWarning("❌ URL NOT FOUND: No client found with URL matching '{Url}'", clientUrl);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ ERROR during API key validation");
                return false;
            }
        }

        private bool IsUrlMatching(string requestUrl, string clientUrl)
        {
            try
            {
                var requestUri = new Uri(requestUrl);
                var clientUri = new Uri(clientUrl);

                // Extract base URL from both URLs (ignore paths like /neoform)
                var requestBaseUrl = $"{requestUri.Scheme}://{requestUri.Host}:{requestUri.Port}";
                var clientBaseUrl = $"{clientUri.Scheme}://{clientUri.Host}:{clientUri.Port}";

                _logger.LogDebug("🔍 URL comparison - Request base: '{RequestBase}' vs Client base: '{ClientBase}' (original client URL: '{OriginalClientUrl}')",
                    requestBaseUrl, clientBaseUrl, clientUrl);

                // Compare base URLs only (ignore paths)
                var isMatch = string.Equals(requestBaseUrl, clientBaseUrl, StringComparison.OrdinalIgnoreCase);

                if (isMatch)
                {
                    _logger.LogDebug("✅ Base URL match: {RequestBase} matches {ClientBase}",
                        requestBaseUrl, clientBaseUrl);
                }
                else
                {
                    _logger.LogDebug("❌ Base URL mismatch: {RequestBase} does not match {ClientBase}",
                        requestBaseUrl, clientBaseUrl);
                }

                return isMatch;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error comparing URLs '{RequestUrl}' with '{ClientUrl}'",
                    requestUrl, clientUrl);
                return false;
            }
        }

    }
}
