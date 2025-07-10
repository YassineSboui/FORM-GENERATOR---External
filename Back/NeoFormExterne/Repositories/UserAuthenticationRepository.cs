using Microsoft.EntityFrameworkCore;
using NeoForm_Externe.Data;
using NeoForm_Externe.Interfaces;
using NeoForm_Externe.Models.Dto;

namespace NeoForm_Externe.Repositories
{
    public class UserAuthenticationRepository : IUserAuthenticationRepository
    {
        private readonly ILogger<UserAuthenticationRepository> _logger;
        private readonly ExternalNeoFormContext _context; // Replace with your actual DbContext

        public UserAuthenticationRepository(ILogger<UserAuthenticationRepository> logger, ExternalNeoFormContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<UserAuthenticationDto.UserAuthentication> GetAuthenticationAsync(string guid)
        {
            try
            {
                return await _context.UserAuthentications
                    .FirstOrDefaultAsync(ua => ua.Guid == guid && ua.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving authentication for GUID: {Guid}", guid);
                return null;
            }
        }

        public async Task StoreAuthenticationAsync(UserAuthenticationDto.UserAuthentication userAuth)
        {
            try
            {
                _context.UserAuthentications.Add(userAuth);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Stored authentication for GUID: {Guid}", userAuth.Guid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing authentication for GUID: {Guid}", userAuth.Guid);
                throw;
            }
        }

        public async Task RevokeAuthenticationAsync(string guid)
        {
            try
            {
                var existingAuths = _context.UserAuthentications
                    .Where(ua => ua.Guid == guid && ua.IsActive);

                foreach (var auth in existingAuths)
                {
                    auth.IsActive = false;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Revoked authentication for GUID: {Guid}", guid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking authentication for GUID: {Guid}", guid);
                throw;
            }
        }

        public async Task<bool> IsAuthenticationValidAsync(string guid)
        {
            try
            {
                var auth = await GetAuthenticationAsync(guid);
                return auth != null && auth.IsActive && auth.ExpiresAt > DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking authentication validity for GUID: {Guid}", guid);
                return false;
            }
        }

        public async Task CleanupExpiredAuthenticationsAsync()
        {
            try
            {
                var expiredAuths = _context.UserAuthentications
                    .Where(ua => ua.IsActive && ua.ExpiresAt <= DateTime.UtcNow);

                foreach (var auth in expiredAuths)
                {
                    auth.IsActive = false;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Cleaned up expired authentications");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired authentications");
            }
        }
    }
}
