using Newtonsoft.Json.Linq;

namespace NeoForm_Externe.Models
{
  
    public class ExecuteQueryResponse
    {
        public IList<string> Columns { get; set; } = new List<string>();
        public IList<JObject> Datas { get; set; } = new List<JObject>();
        public JArray Result { get; set; } = new JArray();
    }

    public class ObjectCount
    {
        public string ObjectType { get; set; }
        public int Count { get; set; }
    }
}