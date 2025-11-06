using NeoForm_Externe.Interfaces;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Serilog;

namespace NeoForm_Externe.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ConfigurationEncryptionService _configEncryptionService;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration, ConfigurationEncryptionService configEncryptionService)
        {
            _logger = logger;
            _configuration = configuration;
            _configEncryptionService = configEncryptionService;
        }

        public async Task<bool> SendOTPEmailAsync(string toEmail, string otp)
        {
            try
            {
                var smtpHost = _configuration["Email:SmtpHost"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                // Use ConfigurationEncryptionService to decrypt encrypted values
                var smtpUsername = _configEncryptionService.GetDecryptedValue("Email:Username");
                var smtpPassword = _configEncryptionService.GetDecryptedValue("Email:Password");
                var fromEmail = _configuration["Email:FromAddress"];
                var fromName = _configuration["Email:FromName"] ?? "Form Access";

                // Validate required email configuration
                if (string.IsNullOrWhiteSpace(smtpHost))
                {
                    _logger.LogError("Email:SmtpHost configuration is missing");
                    return false;
                }
                if (string.IsNullOrWhiteSpace(smtpUsername))
                {
                    _logger.LogError("Email:Username configuration is missing");
                    return false;
                }
                if (string.IsNullOrWhiteSpace(smtpPassword))
                {
                    _logger.LogError("Email:Password configuration is missing");
                    return false;
                }
                if (string.IsNullOrWhiteSpace(fromEmail))
                {
                    _logger.LogError("Email:FromAddress configuration is missing");
                    return false;
                }

                _logger.LogInformation("Email configuration - Host: {Host}, Port: {Port}, From: {From}",
                    smtpHost, smtpPort, fromEmail);

                // Bypass SSL certificate validation for custom SMTP servers
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = "Your Form Access Code",
                    Body = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                            <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px;'>
                                <h2 style='color: #266c87; margin-bottom: 20px;'>Form Access Verification</h2>
                                <p>Your verification code is:</p>
                                <div style='background-color: #fff; padding: 15px; border-radius: 4px; text-align: center; margin: 20px 0;'>
                                    <span style='font-size: 24px; font-weight: bold; letter-spacing: 5px; color: #266c87;'>{otp}</span>
                                </div>
                                <p style='color: #666; font-size: 14px;'>
                                    This code will expire in 5 minutes. If you didn't request this code, please ignore this email.
                                </p>
                                <p style='color: #666; font-size: 12px; margin-top: 30px;'>
                                    This is an automated message, please do not reply.
                                </p>
                            </div>
                        </body>
                        </html>",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);

                _logger.LogInformation("OTP email sent successfully to {Email}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send OTP email to {Email}", toEmail);
                return false;
            }
        }
    }
}