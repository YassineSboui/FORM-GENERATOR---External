using NeoFormExterne.Models;
using NeoForm_Externe.Models;

namespace NeoForm_Externe.Interfaces
{
    public interface ISessionService
    {
        Task<SessionTokenResponse> CreateSessionTokenAsync(SessionTokenRequest request, string ipAddress, string userAgent);
        Task<SwitchClientResponse> SwitchClientAsync(SwitchClientRequest request, string userAgent, string ipAddress);
        Task InvalidateSessionAsync(InvalidateSessionRequest request);
        SessionInfoResponse? GetSessionInfo(string sessionKey);
        Task<SessionAuditHistory> GetAuditHistoryAsync(string? clientId, string? guid, int pageSize, int page);
        Task<SessionStats> GetSessionStatsAsync(string? clientId, int days);
    }
}
