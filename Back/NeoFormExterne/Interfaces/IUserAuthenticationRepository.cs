using NeoForm_Externe.Models.Dto;

namespace NeoForm_Externe.Interfaces
{
    public interface IUserAuthenticationRepository
    {
        Task<UserAuthenticationDto.UserAuthentication> GetAuthenticationAsync(string guid);
        Task StoreAuthenticationAsync(UserAuthenticationDto.UserAuthentication userAuth);
        Task RevokeAuthenticationAsync(string guid);
        Task<bool> IsAuthenticationValidAsync(string guid);
        Task CleanupExpiredAuthenticationsAsync();
    }
}
