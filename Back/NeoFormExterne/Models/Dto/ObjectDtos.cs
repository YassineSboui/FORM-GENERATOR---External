using Newtonsoft.Json;

namespace NeoForm_Externe.Models.Dto
{
    public class ObjectDto
    {
        public int Id { get; set; }
        public string Application { get; set; }
        public string Guid { get; set; }
        public string ObjectName { get; set; }
        public string ObjectType { get; set; }
        public bool IsEncrypted { get; set; }
        public ObjectJsonDto ObjectJson { get; set; }
    }

    public class EncryptedObjectJsonDto
    {
        public string Application { get; set; }
        public string Guid { get; set; }
        public string ObjectName { get; set; }
        public string ObjectType { get; set; }
        public bool IsEncrypted { get; set; }
        public string ObjectConfig { get; set; }
    }

    public class ObjectJsonDto
    {
        public string Application { get; set; }
        public string Guid { get; set; }
        public string ObjectName { get; set; }
        public string ObjectType { get; set; }
        public bool IsEncrypted { get; set; }
        public ConfigDto ObjectConfig { get; set; }
    }

    public class ConfigDto
    {
        public FormConfigDto? FormConfig { get; set; }
        public ExternalApiDto? ExternalApiConfig { get; set; }
        public CollectionDatabaseDto? CollectionDatabaseConfig { get; set; }
        public CollectionQueryDto? CollectionQueryConfig { get; set; }
    }

    public class FormConfigDto
    {
        [JsonProperty("casier")]
        public string RackCode { get; set; } = string.Empty;

        [JsonProperty("sharingMapping")]
        public string MappingName { get; set; } = string.Empty;

        public string FormID { get; set; } = string.Empty;
        public string FormName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IList<string> Models { get; set; } = new List<string>();
        public bool IsReference { get; set; } = false;
    }

    public class CollectionDatabaseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Titre { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TypeBase { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public IList<Param> Params { get; set; } = new List<Param>();
        public string Application { get; set; } = string.Empty;
    }

    public class CollectionQueryDto
    {
        public string DatabaseConfigGuid { get; set; } = string.Empty;
        public string Query { get; set; } = string.Empty;
        public string Titre { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public CustomQueryResult ReturnRes { get; set; } = new CustomQueryResult();
    }

    public class CustomQueryResult
    {
        public IList<Param> Values { get; set; } = new List<Param>();
        public string Root { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public static class ObjectType
    {
        public static readonly string ExternalApi = "API";
        public static readonly string Form = "Form";
    }
}
