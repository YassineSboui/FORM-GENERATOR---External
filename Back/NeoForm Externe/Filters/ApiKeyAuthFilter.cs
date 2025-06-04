using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace NeoForm_Externe.Filters
{
    public class ApiKeyAuthFilter : IAuthorizationFilter
    {
        private const string HeaderName = "X-api-key";
        private readonly string _configuredKey;

        public ApiKeyAuthFilter(IConfiguration configuration)
        {
            _configuredKey = configuration["NeoForm:X-api-key"] ?? "";
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var extractedKey))
            {
                context.Result = new UnauthorizedObjectResult("Missing X-api-key");
                return;
            }

            if (!_configuredKey.Equals(extractedKey))
            {
                context.Result = new UnauthorizedObjectResult("Invalid X-api-key");
            }
        }
    }
}
