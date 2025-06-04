using NeoForm_Externe.Models;

namespace NeoForm_Externe.Interfaces
{
    public interface IExternalSourceService
    {
        Task<ExecuteQueryResponse> ExecuteDbqByObjectName(ExecuteQueryRequest req);

        Task<string> ExecuteApiByObjectName(executeApiRequestByObjectName req);

    }
}
