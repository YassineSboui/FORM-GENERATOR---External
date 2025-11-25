using RestSharp;
using Newtonsoft.Json.Linq;

namespace NeoForm_Externe.Models.Dto
{
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
}
