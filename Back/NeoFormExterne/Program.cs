using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using NeoForm_Externe.Data;
using NeoForm_Externe.Interfaces;
using NeoForm_Externe.Middlewares;
using NeoForm_Externe.Proxy;
using NeoForm_Externe.Services;
using NeoForm_Externe.Services.Authentication;
using NeoForm_Externe.Services.Background;
using NeoForm_Externe.Services.Client;
using NeoForm_Externe.Services.Configuration;
using Serilog;
using Yarp.ReverseProxy.Configuration;
using NeoForm_Externe.Repositories;
using NeoForm_Externe.Core;
using Microsoft.AspNetCore.Identity;
using NeoForm_Externe.Models;

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

// Configure ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ExternalNeoFormContext>()
.AddDefaultTokenProviders();

// Configure JWT for Identity
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        var jwtKey = configEncryptionService.GetDecryptedValue("EmailAuth:JwtSecretKey");
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();


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

    // Initialize SuperAdmin user and roles
    await DatabaseSeeder.InitializeAsync(scope.ServiceProvider);
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
app.UseAuthentication();
app.UseAuthorization();


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

        // ✅ Validate session token for security
        var sessionToken = context.Request.Headers["X-Auth-Session"].FirstOrDefault();

        if (!string.IsNullOrEmpty(sessionToken))
        {
            var emailAuthService = context.RequestServices.GetRequiredService<IEmailAuthService>();
            var validationResult = await emailAuthService.ValidateSessionTokenAsync(sessionToken, guid.ToString(), code.ToString(), clientId);

            if (!validationResult.IsValid)
            {
                Log.Warning("Session token validation failed for guid: {guid}, clientId: {clientId}, error: {error}", guid, clientId, validationResult.Error);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Invalid or expired session token");
                return;
            }

            Log.Information("Session token validated successfully for guid: {guid}, clientId: {clientId}, auth_type: {authType}", guid, clientId, validationResult.AuthType);
        }
        else
        {
            // Session token is required for all proxied requests
            Log.Warning("Missing session token for proxied request to client {clientId}", clientId);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized: Session token required");
            return;
        }

        var tokenService = context.RequestServices.GetRequiredService<TokenService>();
        var token = await tokenService.GetOrRefreshTokenAsync(clientId, baseUrl!, code.ToString(), guid.ToString(), sessionToken);

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
