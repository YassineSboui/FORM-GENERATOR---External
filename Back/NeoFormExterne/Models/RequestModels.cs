using NeoForm_Externe.Models.Dto;

namespace NeoForm_Externe.Models
{
    // API Execution Requests
    public class ExecuteApiRequest
    {
        public int id { get; set; }
        public IList<KeyValue>? Params { get; set; }
    }

    public class ExecuteApiBeforeSave
    {
        public ExternalApiDto ObjectJson { get; set; } = new();
        public string type { get; set; } = string.Empty;
        public IList<Param>? Params { get; set; }
    }

    public class ExecuteApiRequestByObjectName
    {
        public string ObjectName { get; set; } = string.Empty;
        public IList<KeyValue>? Params { get; set; } = new List<KeyValue>();
    }

    // Request Parameters
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

    public class Param
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    // Request Body
    public class BodyRequest
    {
        public string Body { get; set; } = string.Empty;
        public string? content { get; set; }
    }

    // Query Execution
    public class ExecuteQueryRequest
    {
        public string Query { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public IList<Param>? Params { get; set; }
        public string ObjectName { get; set; } = string.Empty;
    }

    public class ExecuteQueryResponse
    {
        public object? Data { get; set; }
        public int Count { get; set; }
        public List<string> Columns { get; set; } = new();
        public List<object> Datas { get; set; } = new();
        public object? Result { get; set; }
    }
}
