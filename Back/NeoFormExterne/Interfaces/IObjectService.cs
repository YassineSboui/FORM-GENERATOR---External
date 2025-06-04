using NeoForm_Externe.Models;
using Newtonsoft.Json.Linq;

namespace NeoForm_Externe.Interfaces
{
    public interface IObjectService
    {
        Task<List<ObjectModels>> GetObjectsByTypeAsync(string objectType);
        Task<ObjectModels?> GetObjectByGuidAsync(string guid);
        Task<ObjectModels> GetObjectByObjectName(string name);
        Task<ObjectModels> PublishObject(JObject obj);
    }
}
