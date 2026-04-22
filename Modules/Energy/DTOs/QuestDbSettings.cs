namespace YopoBackend.Modules.Energy.DTOs
{
    public class QuestDbSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 8812;
        public string Database { get; set; } = "qdb";
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string TableName { get; set; } = "bms_readings";
        public string PowerSensorLikePattern { get; set; } = "%power_kw";
        public string MqttConsumerTableName { get; set; } = "mqtt_consumer";
        public string MqttPointLikePattern { get; set; } = "value";
        public string MqttTopicLikePattern { get; set; } = "%";
        public string DefaultTopicPrefixTemplate { get; set; } = "yopo/{shortName}";
    }
}
