using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using NeoForm_Externe.Data;
using NeoForm_Externe.Interfaces;
using NeoForm_Externe.Middlewares;
using NeoForm_Externe.Proxy;
using NeoForm_Externe.Services;
using Serilog;
using Yarp.ReverseProxy.Configuration;
using NeoForm_Externe.Filters;
using NeoForm_Externe.Repositories;
using NeoForm_Externe.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Register encryption service first
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
builder.Services.AddSingleton<ConfigurationHelper>();
builder.Services.AddSingleton<ConfigurationEncryptionService>();
builder.Services.AddSingleton<ApiKeyMigrationService>();

// Auto-encrypt sensitive values on startup if not already encrypted
var encryptionService = new EncryptionService();
var configEncryptionService = new ConfigurationEncryptionService(
    builder.Configuration,
    encryptionService,
    LoggerFactory.Create(b => b.AddConsole()).CreateLogger<ConfigurationEncryptionService>(),
    builder.Environment
);

// Encrypt configuration values if needed (this will modify appsettings.json on first run)
configEncryptionService.EncryptSensitiveValuesIfNeeded();

// Database - Use decrypted connection string
var connectionString = configEncryptionService.GetDecryptedConnectionString("ExternalNeoFormContext");

builder.Services.AddDbContext<ExternalNeoFormContext>((serviceProvider, options) =>
{
    options.UseSqlServer(connectionString);
    // Note: The DbContext will get IEncryptionService from DI automatically
});

// Services
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IObjectService, ObjectService>();
builder.Services.AddScoped<IExternalSourceService, ExternalSourceService>();
builder.Services.AddScoped<IClientStoreService, ClientStoreService>();
builder.Services.AddScoped<IOidcService, OidcService>();
builder.Services.AddScoped<IUserAuthenticationRepository, UserAuthenticationRepository>();
// Register email authentication services
builder.Services.AddScoped<IEmailAuthService, EmailAuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddMemoryCache(); // For OTP storage
builder.Services.AddSingleton<ClientSessionService>();
builder.Services.AddHostedService<CleanupBackgroundService>();
// Add background service for cleanup
builder.Services.AddHostedService<AuthenticationCleanupService>();
// Register the dynamic API key filter
builder.Services.AddScoped<DynamicApiKeyAuthFilter>();

// Dynamic YARP config with DI-safe singleton provider
builder.Services.AddSingleton<IDynamicClientProvider, DynamicClientProvider>();
builder.Services.AddSingleton<IProxyConfigProvider, CustomProxyConfigProvider>();

// YARP
builder.Services.AddReverseProxy();
builder.Services.AddHttpClient();
builder.Services.AddHttpLogging(_ => { });

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("all", policy =>
    {
        policy.WithOrigins("http://localhost:5174")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Migrate existing API keys to encrypted format on startup
using (var scope = app.Services.CreateScope())
{
    var apiKeyMigrationService = scope.ServiceProvider.GetRequiredService<ApiKeyMigrationService>();
    await apiKeyMigrationService.MigrateApiKeysAsync();
}

app.UseHttpLogging();
// ✅ Supprimer le header X-Frame-Options injecté par défaut
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers.Remove("X-Frame-Options");
        return Task.CompletedTask;
    });

    await next();
});
app.UseRouting();
app.UseMiddleware<GlobalExceptionMiddleware>();


if (app.Environment.IsDevelopment())
{
    app.UseCors("all");

}
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

// Reverse Proxy Routing
app.MapReverseProxy(proxyPipeline =>
{
    proxyPipeline.Use(async (context, next) =>
    {
        var path = context.Request.Path.Value ?? "";

        // ✅ Skip proxy if local
        if (path.StartsWith("/neoformexternal/local/", StringComparison.OrdinalIgnoreCase))
        {
            Log.Information("Bypassing proxy for local path: {path}", path);
            await next();
            return;
        }

        // ✅ Extract clientId
        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid path format");
            return;
        }

        var clientId = parts[1];
        var subPath = "/" + string.Join('/', parts.Skip(2));

        var clientService = context.RequestServices.GetRequiredService<IClientStoreService>();
        if (!clientService.TryGetClient(clientId, out var baseUrl))
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("Unknown client.");
            return;
        }

        var query = QueryHelpers.ParseQuery(context.Request.QueryString.ToString());

        // ✅ Special handling for /auth-type endpoint - no token required
        if (path.Contains("/auth-type", StringComparison.OrdinalIgnoreCase))
        {
            Log.Information("Proxying /auth-type endpoint without authentication for client {clientId}", clientId);
            Log.Information("Original path: {path}, subPath: {subPath}", path, subPath);
            context.Request.Path = subPath;
            context.SetEndpoint(null);
            await next();
            return;
        }

        // ✅ Regular endpoints require token authentication
        if (!query.TryGetValue("code", out var code) || !query.TryGetValue("guid", out var guid))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Missing code or guid in query");
            return;
        }

        var tokenService = context.RequestServices.GetRequiredService<TokenService>();
        var token = await tokenService.GetOrRefreshTokenAsync(clientId, baseUrl!, code.ToString(), guid.ToString());

        // Clean the query
        var cleanQuery = query
            .Where(q => q.Key != "code" && q.Key != "guid")
            .Select(q => new KeyValuePair<string, string?>(q.Key, q.Value.ToString()));

        context.Request.QueryString = QueryString.Create(cleanQuery);
        context.Request.Headers["Authorization"] = $"Bearer {token}";

        // ✅ Path & routing
        context.Request.Path = subPath;
        context.SetEndpoint(null);

        await next();
    });
});
app.Run();
