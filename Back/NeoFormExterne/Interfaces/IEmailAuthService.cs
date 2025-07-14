using System.Security.Claims;

namespace NeoForm_Externe.Interfaces
{
    public interface IEmailAuthService
    {
        Task<EmailValidationResult> ValidateEmailAsync(string email, string guid, string personalCode, string configUrl);
        Task<OTPSendResult> SendOTPAsync(string email, string guid, string personalCode, string configUrl);
        Task<OTPVerificationResult> VerifyOTPAsync(string email, string otp, string guid, string personalCode, string configUrl);
    }

    public class EmailValidationResult
    {
        public bool IsValid { get; set; }
        public string Error { get; set; }
    }

    public class OTPSendResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public int ExpiresInSeconds { get; set; } = 300; // 5 minutes
    }

    public class OTPVerificationResult
    {
        public bool IsValid { get; set; }
        public string Error { get; set; }
        public IEnumerable<Claim> Claims { get; set; }
    }
}