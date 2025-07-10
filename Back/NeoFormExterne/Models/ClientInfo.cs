namespace NeoForm_Externe.Models
{
    public class ClientInfo
    {
        public int Id { get; set; }
        public string ClientId { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }
}
