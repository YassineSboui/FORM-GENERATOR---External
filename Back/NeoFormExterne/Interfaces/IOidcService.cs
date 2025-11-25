using NeoForm_Externe.Models;

namespace NeoForm_Externe.Interfaces
{
    public interface IOidcService
    {
        Task<OidcValidationResult> ValidateOidcCodeAsync(string code, string state, string guid, string configUrl, string personalCode = "");
        Task<bool> StoreUserAuthenticationAsync(string guid, string userId, string userEmail, TokenResponse tokens);
        Task<bool> IsUserAuthenticatedAsync(string guid);
        Task RevokeUserAuthenticationAsync(string guid);
    }
}
