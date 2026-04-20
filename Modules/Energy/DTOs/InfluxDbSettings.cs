namespace YopoBackend.Modules.Energy.DTOs
{
    public class InfluxDbSettings
    {
        public string Url { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Org { get; set; } = string.Empty;
        public string DefaultBucket { get; set; } = string.Empty;
        public string DefaultTopicPrefix { get; set; } = string.Empty;
    }
}
