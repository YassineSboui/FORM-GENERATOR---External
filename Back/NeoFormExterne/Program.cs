using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using NeoForm_Externe.Data;
using NeoForm_Externe.Interfaces;
using NeoForm_Externe.Middlewares;
using NeoForm_Externe.Proxy;
using NeoForm_Externe.Services;
using Serilog;
using Yarp.ReverseProxy.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NeoForm_Externe.Filters;
using Newtonsoft.Json.Linq;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Database
builder.Services.AddDbContext<ExternalNeoFormContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("ExternalNeoFormContext");
    options.UseSqlServer(connectionString);
});

// Services
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IObjectService, ObjectService>();
builder.Services.AddScoped<IExternalSourceService, ExternalSourceService>();
builder.Services.AddScoped<IClientStoreService, ClientStoreService>();

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
// ✅ Authentification via JWT (Keycloak)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Jwt:Authority"];
        options.Audience = builder.Configuration["Jwt:Audience"];
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role // Use ClaimTypes.Role here
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var identity = context.Principal?.Identity as ClaimsIdentity;
                var resourceAccessClaim = context.Principal?.FindFirst("resource_access");

                if (resourceAccessClaim != null)
                {
                    var resourceAccess = JObject.Parse(resourceAccessClaim.Value);
                    var roles = resourceAccess?["NeoFormExt"]?["roles"]?.ToObject<List<string>>();

                    if (roles != null)
                    {
                        foreach (var role in roles)
                        {
                            identity?.AddClaim(new Claim(ClaimTypes.Role, role));
                        }
                    }
                }

                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

app.UseHttpLogging();
app.UseRouting();
app.UseMiddleware<GlobalExceptionMiddleware>();


if (app.Environment.IsDevelopment())
{
    app.UseCors("all");

}
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
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

        // ✅ Skip token if local
        if (!query.TryGetValue("code", out var code) || !query.TryGetValue("guid", out var guid))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Missing code or guid in query");
            return;
        }

        var tokenService = context.RequestServices.GetRequiredService<TokenService>();
        var token = await tokenService.GetOrRefreshTokenAsync(clientId, baseUrl, code.ToString(), guid.ToString());

        // Clean the query
        var cleanQuery = query
            .Where(q => q.Key != "code" && q.Key != "guid")
            .ToDictionary(q => q.Key, q => q.Value.ToString());

        context.Request.QueryString = QueryString.Create(cleanQuery);
        context.Request.Headers["Authorization"] = $"Bearer {token}";

        // ✅ Path & routing
        context.Request.Path = subPath;
        context.SetEndpoint(null);

        await next();
    });
});

app.Run();
