using NeoForm_Externe.Models;
using Newtonsoft.Json.Linq;

namespace NeoForm_Externe.Interfaces
{
    public interface IObjectService
    {
        Task<List<ObjectModels>> GetObjectsByTypeAsync(string objectType);
        Task<ObjectModels?> GetObjectByGuidAsync(string guid , bool nullifySensitiveFields = true);
        Task<ObjectModels> GetObjectByObjectName(string name, bool nullifySensitiveFields = true);
        Task<ObjectModels> PublishObject(JObject obj);
    }
}
