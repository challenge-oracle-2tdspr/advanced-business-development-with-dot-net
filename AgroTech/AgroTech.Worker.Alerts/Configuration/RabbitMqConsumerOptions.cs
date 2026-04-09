namespace AgroTech.Worker.Alerts.Configuration
{
    public class RabbitMqConsumerOptions
    {
        public const string SectionName = "RabbitMq";

        public bool Enabled { get; set; } = true;

        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
        public string ClientProvidedName { get; set; } = "agrotech-worker-alerts";

        public string Exchange { get; set; } = "agrotech.events";
        public string Queue { get; set; } = "agrotech.alerts.queue";
        public string RoutingKey { get; set; } = "sensor.reading.created";
    }
}