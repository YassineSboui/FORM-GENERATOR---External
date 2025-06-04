using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;

namespace NeoForm_Externe.Models
{
    public partial class ObjectModels
    {
        public int Id { get; set; }

        public string? Guid { get; set; }

        public string? Application { get; set; }

        public string? ObjectName { get; set; }

        public string? ObjectType { get; set; }

        public bool IsEncrypted { get; set; }

        public string ObjectJson { get; set; } = string.Empty;

        public ObjectDto ToObjectDto()
        {
            return new ObjectDto()
            {
                Id = Id,
                Application = Application,
                Guid = Guid,
                ObjectName = ObjectName,
                ObjectType = ObjectType,
                IsEncrypted = IsEncrypted,
                ObjectJson = JsonConvert.DeserializeObject<ObjectJsonDto>(ObjectJson)
            };
        }
    }
}
