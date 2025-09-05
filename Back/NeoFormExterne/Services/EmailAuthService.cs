using Microsoft.Extensions.Caching.Memory;
using NeoForm_Externe.Interfaces;
using NeoForm_Externe.Models;
using RestSharp;
using Serilog;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.Json;
using static NeoForm_Externe.Models.OidcModels;

namespace NeoForm_Externe.Services
{
    public class EmailAuthService : IEmailAuthService
    {
        private readonly ILogger<EmailAuthService> _logger;
        private readonly IMemoryCache _cache;
        private readonly IEmailService _emailService;
        private readonly Dictionary<string, DateTime> _rateLimitTracker;
        private readonly Dictionary<string, int> _attemptTracker;

        public EmailAuthService(
            ILogger<EmailAuthService> logger,
            IMemoryCache cache,
            IEmailService emailService)
        {
            _logger = logger;
            _cache = cache;
            _emailService = emailService;
            _rateLimitTracker = new Dictionary<string, DateTime>();
            _attemptTracker = new Dictionary<string, int>();
        }

        public async Task<EmailValidationResult> ValidateEmailAsync(string email, string guid, string personalCode, string configUrl)
        {
            try
            {
                // Check rate limiting
                if (IsRateLimited(email))
                {
                    return new EmailValidationResult
                    {
                        IsValid = false,
                        Error = "Too many attempts. Please try again later."
                    };
                }

                // Get invitation configuration
                var config = await GetInvitationConfigAsync(guid, personalCode, configUrl);
                if (config == null)
                {
                    return new EmailValidationResult
                    {
                        IsValid = false,
                        Error = "Invalid invitation configuration"
                    };
                }

                // Validate email format
                if (!IsValidEmail(email))
                {
                    return new EmailValidationResult
                    {
                        IsValid = false,
                        Error = "Invalid email format"
                    };
                }

                // Check if email is in the whitelist
                var isWhitelisted = config.Emails?.Any(e =>
                    string.Equals(e, email, StringComparison.OrdinalIgnoreCase)) ?? config.allEmailsAllowed ?? false;

                if (!isWhitelisted)
                {
                    TrackFailedAttempt(email);
                    _logger.LogWarning("Email {Email} not found in whitelist for GUID: {Guid}", email, guid);
                    return new EmailValidationResult
                    {
                        IsValid = false,
                        Error = "Email not authorized for this form"
                    };
                }

                _logger.LogInformation("Email {Email} validated successfully for GUID: {Guid}", email, guid);
                return new EmailValidationResult { IsValid = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating email {Email} for GUID: {Guid}", email, guid);
                return new EmailValidationResult
                {
                    IsValid = false,
                    Error = "Validation error occurred"
                };
            }
        }

        public async Task<OTPSendResult> SendOTPAsync(string email, string guid, string personalCode, string configUrl)
        {
            try
            {
                // First validate the email again
                var validation = await ValidateEmailAsync(email, guid, personalCode, configUrl);
                if (!validation.IsValid)
                {
                    return new OTPSendResult
                    {
                        Success = false,
                        Error = validation.Error
                    };
                }

                // Generate 6-digit OTP
                var otp = GenerateOTP();
                var cacheKey = $"otp_{email}_{guid}_{personalCode}";
                var expiresIn = TimeSpan.FromMinutes(5);

                // Store OTP in cache with expiration
                _cache.Set(cacheKey, otp, expiresIn);

                // Send email
                var emailSent = await _emailService.SendOTPEmailAsync(email, otp);
                if (!emailSent)
                {
                    return new OTPSendResult
                    {
                        Success = false,
                        Error = "Failed to send email"
                    };
                }

                _logger.LogInformation("OTP sent successfully to {Email} for GUID: {Guid}", email, guid);
                return new OTPSendResult
                {
                    Success = true,
                    ExpiresInSeconds = (int)expiresIn.TotalSeconds
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP to {Email} for GUID: {Guid}", email, guid);
                return new OTPSendResult
                {
                    Success = false,
                    Error = "Failed to send OTP"
                };
            }
        }

        public async Task<OTPVerificationResult> VerifyOTPAsync(string email, string otp, string guid, string personalCode, string configUrl)
        {
            try
            {
                // Check rate limiting for OTP verification
                if (IsRateLimited($"otp_{email}"))
                {
                    return new OTPVerificationResult
                    {
                        IsValid = false,
                        Error = "Too many OTP attempts. Please request a new code."
                    };
                }

                var cacheKey = $"otp_{email}_{guid}_{personalCode}";

                // Get stored OTP from cache
                if (!_cache.TryGetValue(cacheKey, out string storedOtp))
                {
                    return new OTPVerificationResult
                    {
                        IsValid = false,
                        Error = "OTP expired or not found"
                    };
                }

                // Verify OTP
                if (storedOtp != otp)
                {
                    TrackFailedAttempt($"otp_{email}");
                    return new OTPVerificationResult
                    {
                        IsValid = false,
                        Error = "Invalid OTP code"
                    };
                }

                // Remove OTP from cache after successful verification
                _cache.Remove(cacheKey);

                // Create claims for the authenticated user
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.NameIdentifier, email),
                    new Claim("auth_type", "invitation"),
                    new Claim("guid", guid),
                    new Claim("personal_code", personalCode),
                    new Claim("verified_at", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
                };

                _logger.LogInformation("OTP verified successfully for {Email} for GUID: {Guid}", email, guid);
                return new OTPVerificationResult
                {
                    IsValid = true,
                    Claims = claims
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP for {Email} for GUID: {Guid}", email, guid);
                return new OTPVerificationResult
                {
                    IsValid = false,
                    Error = "OTP verification failed"
                };
            }
        }

        private async Task<InvitationConfig> GetInvitationConfigAsync(string guid, string personalCode, string configUrl)
        {
            try
            {
                // Use the same endpoint pattern as OIDC authentication
                var client = new RestClient();
                var request = new RestRequest(configUrl);
                request.AddQueryParameter("guid", guid);
                request.AddQueryParameter("code", personalCode);

                var response = await client.ExecuteAsync(request);

                if (!response.IsSuccessful)
                {
                    _logger.LogWarning("Failed to get invitation config. Status: {StatusCode}, Content: {Content}",
                        response.StatusCode, response.Content);
                    return null;
                }

                var authResponse = JsonSerializer.Deserialize<AuthTypeResponse>(response.Content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (authResponse?.Valid == true && authResponse.AuthType == "invitation" && authResponse.AuthConfig != null)
                {
                    return new InvitationConfig
                    {
                        Emails = authResponse.AuthConfig.Emails,
                        VerifyEmail = authResponse.AuthConfig.VerifyEmail
                    };
                }

                if (authResponse?.Valid == true && authResponse.AuthType == "otp")
                {
                    return new InvitationConfig
                    {
                        allEmailsAllowed = true,
                        VerifyEmail = true
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving invitation configuration for GUID: {Guid}", guid);
                return null;
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateOTP()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private bool IsRateLimited(string key)
        {
            var now = DateTime.UtcNow;

            // Check if we have attempts for this key
            if (_attemptTracker.ContainsKey(key))
            {
                // If we have too many attempts and not enough time has passed
                if (_attemptTracker[key] >= 3 && _rateLimitTracker.ContainsKey(key))
                {
                    var timeSinceLastAttempt = now - _rateLimitTracker[key];
                    if (timeSinceLastAttempt < TimeSpan.FromHours(1))
                    {
                        return true;
                    }
                    else
                    {
                        // Reset counters after cooldown period
                        _attemptTracker[key] = 0;
                        _rateLimitTracker.Remove(key);
                    }
                }
            }

            return false;
        }

        private void TrackFailedAttempt(string key)
        {
            var now = DateTime.UtcNow;

            if (!_attemptTracker.ContainsKey(key))
            {
                _attemptTracker[key] = 0;
            }

            _attemptTracker[key]++;
            _rateLimitTracker[key] = now;
        }
    }

    // Supporting models
    public class InvitationConfig
    {
        public string[] Emails { get; set; }
        public bool VerifyEmail { get; set; }
        public bool? allEmailsAllowed { get; set; }
    }
}