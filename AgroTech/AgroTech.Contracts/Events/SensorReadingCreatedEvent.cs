namespace AgroTech.Contracts.Events
{
    public class SensorReadingCreatedEvent
    {
        public string EventName { get; init; } = "sensor.reading.created";
        public string CorrelationId { get; init; } = string.Empty;

        public Guid SensorId { get; init; }

        public string SensorName { get; init; } = string.Empty;
        public int SensorType { get; init; }

        public double Value { get; init; }
        public DateTime Timestamp { get; init; }

        public string Source { get; init; } = "node-red";

        public string? FieldId { get; init; }
        public string? FarmId { get; init; }
        public string? ZoneId { get; init; }
    }
}