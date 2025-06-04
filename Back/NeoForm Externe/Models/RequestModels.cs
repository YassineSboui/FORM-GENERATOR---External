
using RestSharp;
using Newtonsoft.Json.Linq;
namespace NeoForm_Externe.Models
{   
    
    public class KeyValue
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
    public class KeyValuePathParams
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Type { get; set; }
    }
    public class BodyRequest
    {
        public bool? createfile { get; set; } = false;
        public string body_input { get; set; } = string.Empty;
        public string content { get; set; } = string.Empty;
        public string content_type { get; set; } = string.Empty;
    }
    public class ExternalApiDto
    {
        public string BaseUrl { get; set; } = string.Empty;
        public Method Method { get; set; } = Method.Get;
        public IList<KeyValue>? Headers { get; set; }
        public IList<KeyValuePathParams>? Parameters { get; set; }
        public IList<KeyValue>? Queries { get; set; }
        public JObject? JsonBody { get; set; }
        public IList<KeyValue>? FormData { get; set; }
        public BodyRequest? BodyRequest { get; set; }
        public Authorization? Authorization { get; set; }
    }
    public class Param
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
    public class ExecuteApiRequest
    {
        public int id { get; set; }
        public IList<KeyValue>? Params { get; set; }
    }
    public class ExecuteApiBeforeSave
    {
        public ExternalApiDto ObjectJson { get; set; }
        public string type { get; set; }
        public IList<Param>? Params { get; set; }
    }
    public class executeApiRequestByObjectName
    {
        public string ObjectName { get; set; }
        public IList<KeyValue>? Params { get; set; } = new List<KeyValue>();
    }
    public class Authorization
    {
        public string ApiType;
        public int? api_type { get; set; }
        public string? key { get; set; }
        public string? value { get; set; }
        public string? bearerToken { get; set; }
        public string? algorithm { get; set; }
        public string? secret { get; set; }
        public string? secret_encoded { get; set; }
        public string? payload { get; set; }
        public string? username { get; set; }
        public string? password { get; set; }

    }

}
