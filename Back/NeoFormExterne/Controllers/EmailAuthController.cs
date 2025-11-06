using Microsoft.AspNetCore.Mvc;
using NeoForm_Externe.Interfaces;
using NeoForm_Externe.Models;
using NeoForm_Externe.Models.Dto;
using Serilog;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

namespace NeoForm_Externe.Controllers
{
    [ApiController]
    [Route("neoformexternal/local/api/auth")]
    public class EmailAuthController : ControllerBase
    {
        private readonly ILogger<EmailAuthController> _logger;
        private readonly IEmailAuthService _emailAuthService;
        private readonly IUserAuthenticationRepository _authRepository;
        private readonly IConfiguration _configuration;

        public EmailAuthController(
            ILogger<EmailAuthController> logger,
            IEmailAuthService emailAuthService,
            IUserAuthenticationRepository authRepository,
            IConfiguration configuration)
        {
            _logger = logger;
            _emailAuthService = emailAuthService;
            _authRepository = authRepository;
            _configuration = configuration;
        }

        [HttpPost("validate-email")]
        public async Task<IActionResult> ValidateEmail([FromBody] EmailValidationRequest request)
        {
            try
            {
                _logger.LogInformation("Email validation request for GUID: {Guid}, Personal Code: {PersonalCode}, Email: {Email}",
                    request.Guid, request.PersonalCode, request.Email);
                Log.Information("Email validation request for GUID: {Guid}, Personal Code: {PersonalCode}, Email: {Email}",
                    request.Guid, request.PersonalCode, request.Email);

                var result = await _emailAuthService.ValidateEmailAsync(request.Email, request.Guid, request.PersonalCode, request.ConfigUrl);

                if (result.IsValid)
                {
                    _logger.LogInformation("Email validation successful for {Email}", request.Email);
                    return Ok(new { valid = true, message = "Email authorized" });
                }
                else
                {
                    _logger.LogWarning("Email validation failed for {Email}: {Error}", request.Email, result.Error);
                    return BadRequest(new { valid = false, message = result.Error });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating email for GUID: {Guid}", request.Guid);
                return StatusCode(500, new { valid = false, message = "Internal server error" });
            }
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOTP([FromBody] SendOTPRequest request)
        {
            try
            {
                _logger.LogInformation("OTP send request for Email: {Email}, GUID: {Guid}", request.Email, request.Guid);
                Log.Information("OTP send request for Email: {Email}, GUID: {Guid}", request.Email, request.Guid);

                var result = await _emailAuthService.SendOTPAsync(request.Email, request.Guid, request.PersonalCode, request.ConfigUrl);

                if (result.Success)
                {
                    _logger.LogInformation("OTP sent successfully to {Email}", request.Email);
                    return Ok(new { success = true, message = "OTP sent successfully", expiresIn = 300 });
                }
                else
                {
                    _logger.LogWarning("Failed to send OTP to {Email}: {Error}", request.Email, result.Error);
                    return BadRequest(new { success = false, message = result.Error });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP for Email: {Email}", request.Email);
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOTP([FromBody] VerifyOTPRequest request)
        {
            try
            {
                _logger.LogInformation("OTP verification request for Email: {Email}, GUID: {Guid}", request.Email, request.Guid);
                Log.Information("OTP verification request for Email: {Email}, GUID: {Guid}", request.Email, request.Guid);

                var result = await _emailAuthService.VerifyOTPAsync(request.Email, request.Otp, request.Guid, request.PersonalCode, request.ConfigUrl);

                if (result.IsValid)
                {
                    // Generate secure JWT token
                    var secureToken = GenerateJwtToken(request.Email, request.Guid, request.PersonalCode);

                    // Store authentication with the secure token
                    var authStored = await StoreEmailAuthenticationAsync(request.Guid, request.Email, result.Claims, secureToken);

                    if (authStored)
                    {
                        _logger.LogInformation("OTP verification and authentication storage successful for {Email}", request.Email);
                        return Ok(new
                        {
                            valid = true,
                            message = "OTP verified successfully",
                            token = secureToken,
                            expiresIn = 28800 // 8 hours in seconds
                        });
                    }
                    else
                    {
                        _logger.LogError("Failed to store authentication for {Email}", request.Email);
                        return StatusCode(500, new { valid = false, message = "Failed to store authentication" });
                    }
                }
                else
                {
                    _logger.LogWarning("OTP verification failed for {Email}: {Error}", request.Email, result.Error);
                    return BadRequest(new { valid = false, message = result.Error });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP for Email: {Email}", request.Email);
                return StatusCode(500, new { valid = false, message = "Internal server error" });
            }
        }

        [HttpPost("validate-token")]
        public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest request)
        {
            try
            {
                _logger.LogInformation("Token validation request for GUID: {Guid}, Email: {Email}",
                    request.Guid, request.Email);

                // Validate required parameters
                if (string.IsNullOrWhiteSpace(request.Token) ||
                    string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Guid) ||
                    string.IsNullOrWhiteSpace(request.PersonalCode))
                {
                    return BadRequest(new { valid = false, message = "Missing required parameters" });
                }

                // Validate JWT token
                var tokenValidation = ValidateJwtToken(request.Token);
                if (!tokenValidation.IsValid)
                {
                    return BadRequest(new { valid = false, message = tokenValidation.Error });
                }

                // Extract claims from token
                var tokenClaims = tokenValidation.Claims?.ToList() ?? new List<Claim>();

                // Validate token claims match request parameters
                var tokenEmail = tokenClaims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var tokenGuid = tokenClaims.FirstOrDefault(c => c.Type == "guid")?.Value;
                var tokenPersonalCode = tokenClaims.FirstOrDefault(c => c.Type == "personal_code")?.Value;

                if (tokenEmail != request.Email || tokenGuid != request.Guid || tokenPersonalCode != request.PersonalCode)
                {
                    _logger.LogWarning("Token validation failed - parameter mismatch for {Email}", request.Email);
                    return BadRequest(new { valid = false, message = "Token parameters do not match request" });
                }

                // Check if authentication exists and is still valid
                var existingAuth = await _authRepository.GetAuthenticationAsync(request.Guid);
                if (existingAuth == null || !existingAuth.IsActive || existingAuth.ExpiresAt < DateTime.UtcNow)
                {
                    _logger.LogWarning("Token validation failed - no valid authentication found for GUID: {Guid}", request.Guid);
                    return BadRequest(new { valid = false, message = "Authentication session expired or invalid" });
                }

                // Additional security check: verify stored token matches
                if (existingAuth.AccessToken != request.Token)
                {
                    _logger.LogWarning("Token validation failed - token mismatch for GUID: {Guid}", request.Guid);
                    return BadRequest(new { valid = false, message = "Invalid token" });
                }

                _logger.LogInformation("Token validation successful for {Email}, GUID: {Guid}", request.Email, request.Guid);

                return Ok(new
                {
                    valid = true,
                    message = "Token is valid",
                    expiresAt = existingAuth.ExpiresAt,
                    email = existingAuth.UserEmail
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token for GUID: {Guid}", request.Guid);
                return StatusCode(500, new { valid = false, message = "Internal server error" });
            }
        }

        private TokenValidationResult ValidateJwtToken(string token)
        {
            try
            {
                var jwtKey = _configuration["EmailAuth:JwtSecretKey"] ?? "your-256-bit-secret-key-here-make-it-secure-and-long-enough-for-production";
                var key = Encoding.ASCII.GetBytes(jwtKey);

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = "NeoFormExternal",
                    ValidateAudience = true,
                    ValidAudience = "NeoFormExternal",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

                return new TokenValidationResult
                {
                    IsValid = true,
                    Claims = principal.Claims
                };
            }
            catch (SecurityTokenExpiredException)
            {
                return new TokenValidationResult { IsValid = false, Error = "Token has expired" };
            }
            catch (SecurityTokenException ex)
            {
                return new TokenValidationResult { IsValid = false, Error = $"Token validation failed: {ex.Message}" };
            }
            catch (Exception ex)
            {
                return new TokenValidationResult { IsValid = false, Error = $"Token validation error: {ex.Message}" };
            }
        }

        private string GenerateJwtToken(string email, string guid, string personalCode)
        {
            try
            {
                var jwtKey = _configuration["EmailAuth:JwtSecretKey"] ?? "your-256-bit-secret-key-here-make-it-secure-and-long-enough-for-production";
                var key = Encoding.ASCII.GetBytes(jwtKey);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Email, email),
                        new Claim("guid", guid),
                        new Claim("personal_code", personalCode),
                        new Claim("auth_type", "email"),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                    }),
                    Expires = DateTime.UtcNow.AddHours(8),
                    Issuer = "NeoFormExternal",
                    Audience = "NeoFormExternal",
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token for {Email}", email);
                throw;
            }
        }

        private async Task<bool> StoreEmailAuthenticationAsync(string guid, string email, IEnumerable<System.Security.Claims.Claim> claims, string secureToken)
        {
            try
            {
                var userAuth = new UserAuthenticationDto.UserAuthentication
                {
                    Id = Guid.NewGuid().ToString(),
                    Guid = guid,
                    UserId = email, // Use email as userId for invitation auth
                    UserEmail = email,
                    AccessToken = secureToken, // Use the secure JWT token
                    IdToken = null,
                    RefreshToken = null,
                    ExpiresAt = DateTime.UtcNow.AddHours(8), // 8 hour session
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                // Remove any existing authentication for this GUID
                await _authRepository.RevokeAuthenticationAsync(guid);

                // Store the new authentication
                await _authRepository.StoreAuthenticationAsync(userAuth);

                _logger.LogInformation("Stored email authentication for GUID: {Guid}, Email: {Email}", guid, email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing email authentication for GUID: {Guid}", guid);
                return false;
            }
        }
    }

    // Request models
    public class EmailValidationRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Guid { get; set; }

        [Required]
        public string PersonalCode { get; set; }

        [Required]
        public string ConfigUrl { get; set; }
    }

    public class SendOTPRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Guid { get; set; }

        [Required]
        public string PersonalCode { get; set; }

        [Required]
        public string ConfigUrl { get; set; }
    }

    public class VerifyOTPRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP must be 6 digits")]
        public string Otp { get; set; }

        [Required]
        public string Guid { get; set; }

        [Required]
        public string PersonalCode { get; set; }

        [Required]
        public string ConfigUrl { get; set; }
    }

    public class ValidateTokenRequest
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Guid { get; set; }

        [Required]
        public string PersonalCode { get; set; }
    }

    public class TokenValidationResult
    {
        public bool IsValid { get; set; }
        public string Error { get; set; }
        public IEnumerable<Claim> Claims { get; set; }
    }
}