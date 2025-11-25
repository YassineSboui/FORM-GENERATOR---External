using System.Security.Claims;
using NeoForm_Externe.Models;

namespace NeoForm_Externe.Interfaces
{
    public interface IEmailAuthService
    {
        Task<EmailValidationResult> ValidateEmailAsync(string email, string guid, string personalCode, string configUrl);
        Task<OTPSendResult> SendOTPAsync(string email, string guid, string personalCode, string configUrl);
        Task<OTPVerificationResult> VerifyOTPAsync(string email, string otp, string guid, string personalCode, string configUrl);
        Task<SessionTokenValidationResult> ValidateSessionTokenAsync(string token, string guid, string code, string clientId);
    }
}