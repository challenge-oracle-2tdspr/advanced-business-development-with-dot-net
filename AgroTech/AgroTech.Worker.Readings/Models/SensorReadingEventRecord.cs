namespace AgroTech.Worker.Readings.Models
{
    public class SensorReadingEventRecord
    {
        public string EventKey { get; set; } = string.Empty;
        public string ReadingId { get; set; } = string.Empty;
        public string? CorrelationId { get; set; }

        public string? SensorId { get; set; }
        public string? SensorCode { get; set; }

        public int SensorTypeId { get; set; }
        public string SensorTypeName { get; set; } = string.Empty;

        public double MetricValue { get; set; }

        public string? SourceName { get; set; }
        public string? FarmId { get; set; }
        public string? FieldId { get; set; }
        public string? ZoneId { get; set; }

        public DateTime OccurredAt { get; set; }
        public string PayloadJson { get; set; } = "{}";
    }
}
