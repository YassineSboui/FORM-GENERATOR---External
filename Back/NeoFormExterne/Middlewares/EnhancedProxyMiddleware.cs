using Microsoft.AspNetCore.WebUtilities;
using NeoForm_Externe.Interfaces;
using NeoForm_Externe.Services.Authentication;
using NeoForm_Externe.Services.Client;
using Serilog;
using System.Net;

namespace NeoForm_Externe.Middlewares
{
    public class EnhancedProxyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<EnhancedProxyMiddleware> _logger;

        public EnhancedProxyMiddleware(RequestDelegate next, ILogger<EnhancedProxyMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "";

            // Skip if not a proxy path or if it's a local controller route
            if (!path.StartsWith("/neoformexternal/", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/neoformexternal/local/", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/neoformexternal/Clients", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/neoformexternal/Objects", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            try
            {
                await HandleProxyRequest(context);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized access attempt");
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await context.Response.WriteAsync(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in proxy middleware");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync("Internal server error");
            }
        }

        private async Task HandleProxyRequest(HttpContext context)
        {
            var path = context.Request.Path.Value ?? "";
            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync("Invalid path format. Expected: /neoformexternal/{clientId}/...");
                return;
            }

            var clientId = parts[1];
            var subPath = "/" + string.Join('/', parts.Skip(2));

            // Validate client exists
            var clientService = context.RequestServices.GetRequiredService<IClientStoreService>();
            if (!clientService.TryGetClient(clientId, out var baseUrl) || string.IsNullOrEmpty(baseUrl))
            {
                _logger.LogWarning($"Unknown client requested: {clientId}");
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await context.Response.WriteAsync($"Unknown client: {clientId}");
                return;
            }

            // Handle session management
            var sessionService = context.RequestServices.GetRequiredService<ClientSessionService>();
            var userAgent = context.Request.Headers.UserAgent.ToString();
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var sessionKey = sessionService.GetOrCreateSession(clientId, userAgent, ipAddress);

            // Add session header for client tracking
            context.Request.Headers["X-Client-Session"] = sessionKey;

            // Parse query parameters
            var query = QueryHelpers.ParseQuery(context.Request.QueryString.ToString());

            // Check for required authentication parameters
            if (!query.TryGetValue("code", out var code) || !query.TryGetValue("guid", out var guid))
            {
                _logger.LogWarning($"Missing authentication parameters for client {clientId}");
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync("Missing required authentication parameters (code and guid)");
                return;
            }

            // Handle token authentication
            var tokenService = context.RequestServices.GetRequiredService<TokenService>();

            try
            {
                var token = await tokenService.GetOrRefreshTokenAsync(clientId, baseUrl, code.ToString(), guid.ToString());

                // Clean the query string (remove auth parameters)
                var cleanQuery = query
                    .Where(q => q.Key != "code" && q.Key != "guid")
                    .Select(q => new KeyValuePair<string, string?>(q.Key, q.Value.ToString()));

                context.Request.QueryString = QueryString.Create(cleanQuery);
                context.Request.Headers.Authorization = $"Bearer {token}";

                // Update path for routing
                context.Request.Path = subPath;
                context.SetEndpoint(null);

                _logger.LogDebug($"Successfully authenticated request for client {clientId} to path {subPath}");
            }
            catch (UnauthorizedAccessException)
            {
                // Invalidate session on auth failure
                sessionService.InvalidateSession(sessionKey);
                throw;
            }

            await _next(context);
        }
    }
}
