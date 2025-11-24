using System.Security.Claims;

namespace NeoForm_Externe.Interfaces
{
    public interface IEmailAuthService
    {
        Task<EmailValidationResult> ValidateEmailAsync(string email, string guid, string personalCode, string configUrl);
        Task<OTPSendResult> SendOTPAsync(string email, string guid, string personalCode, string configUrl);
        Task<OTPVerificationResult> VerifyOTPAsync(string email, string otp, string guid, string personalCode, string configUrl);
        Task<SessionTokenValidationResult> ValidateSessionTokenAsync(string token, string guid, string code, string clientId);
    }

    public class EmailValidationResult
    {
        public bool IsValid { get; set; }
        public string? Error { get; set; }
    }

    public class OTPSendResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public int ExpiresInSeconds { get; set; } = 300; // 5 minutes
    }

    public class OTPVerificationResult
    {
        public bool IsValid { get; set; }
        public string? Error { get; set; }
        public IEnumerable<Claim>? Claims { get; set; }
    }

    public class SessionTokenRequest
    {
        public string Guid { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string AuthType { get; set; } = string.Empty; // "otp", "oidc", "none"
        public string Email { get; set; } = string.Empty;
        public string Sub { get; set; } = string.Empty; // user identifier (email for OTP, user ID for OIDC, "anonymous" for none)
    }

    public class SessionTokenValidationResult
    {
        public bool IsValid { get; set; }
        public string? Error { get; set; }
        public string Guid { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string AuthType { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Sub { get; set; } = string.Empty;
    }
}