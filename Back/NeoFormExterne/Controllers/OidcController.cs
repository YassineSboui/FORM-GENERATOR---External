using Microsoft.AspNetCore.Mvc;
using NeoForm_Externe.Interfaces;

namespace NeoForm_Externe.Controllers
{
    [ApiController]
    [Route("neoformexternal/local/api/auth")]
    public class OidcController : ControllerBase
    {
        private readonly IOidcService _oidcService;
        private readonly ILogger<OidcController> _logger;

        public OidcController(IOidcService oidcService, ILogger<OidcController> logger)
        {
            _oidcService = oidcService;
            _logger = logger;
        }

        public class OidcValidationRequest
        {
            public string Code { get; set; }
            public string State { get; set; }
            public string Guid { get; set; }
            public string ConfigUrl { get; set; }
            public string PersonalCode { get; set; } // Add personal code property
        }

        [HttpPost("validate-oidc")]
        public async Task<IActionResult> ValidateOidc([FromBody] OidcValidationRequest request)
        {
            try
            {
                _logger.LogInformation("OIDC validation request for GUID: {Guid}", request.Guid);

                if (string.IsNullOrEmpty(request.Code) || string.IsNullOrEmpty(request.State) ||
                    string.IsNullOrEmpty(request.Guid) || string.IsNullOrEmpty(request.ConfigUrl))
                {
                    return BadRequest(new { success = false, error = "Missing required parameters" });
                }

                var result = await _oidcService.ValidateOidcCodeAsync(
                    request.Code, request.State, request.Guid, request.ConfigUrl, request.PersonalCode);

                if (!result.IsValid)
                {
                    return BadRequest(new { success = false, error = result.Error });
                }

                // Store the authentication
                var stored = await _oidcService.StoreUserAuthenticationAsync(
                    request.Guid, result.UserId, result.UserEmail, result.Tokens);

                if (!stored)
                {
                    return BadRequest(new { success = false, error = "Failed to store authentication" });
                }

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OIDC validation for GUID: {Guid}", request.Guid);
                return BadRequest(new { success = false, error = "Authentication validation failed" });
            }
        }

        [HttpGet("check/{guid}")]
        public async Task<IActionResult> CheckAuthentication(string guid)
        {
            try
            {
                var isAuthenticated = await _oidcService.IsUserAuthenticatedAsync(guid);
                return Ok(new { authenticated = isAuthenticated });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking authentication for GUID: {Guid}", guid);
                return BadRequest(new { authenticated = false, error = "Failed to check authentication" });
            }
        }

        [HttpPost("revoke/{guid}")]
        public async Task<IActionResult> RevokeAuthentication(string guid)
        {
            try
            {
                await _oidcService.RevokeUserAuthenticationAsync(guid);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking authentication for GUID: {Guid}", guid);
                return BadRequest(new { success = false, error = "Failed to revoke authentication" });
            }
        }
    }
}
