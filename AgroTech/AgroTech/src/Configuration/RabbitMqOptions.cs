namespace AgroTech.Configuration
{
    public class RabbitMqOptions
    {
        public const string SectionName = "RabbitMq";

        public bool Enabled { get; set; } = true;

        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
        public string ClientProvidedName { get; set; } = "agrotech-api.publisher";
        public string Exchange { get; set; } = "agrotech.events";
        public string AlertsQueue { get; set; } = "agrotech.alerts.queue";
        public string RecommendationsQueue { get; set; } = "agrotech.recommendations.queue";
        public string ReadingsQueue { get; set; } = "agrotech.readings.queue";
        public string SensorReadingCreatedRoutingKey { get; set; } = "sensor.reading.created";
        public string Source { get; set; } = "node-red";
    }
}