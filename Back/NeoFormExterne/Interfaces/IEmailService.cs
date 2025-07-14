namespace NeoForm_Externe.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendOTPEmailAsync(string toEmail, string otp);
    }
}