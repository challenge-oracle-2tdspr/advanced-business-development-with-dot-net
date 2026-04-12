namespace AgroTech.Worker.Alerts.Models
{
    public class AlertEventRecord
    {
        public string EventKey { get; set; } = string.Empty;
        public string ReadingId { get; set; } = string.Empty;
        public string? CorrelationId { get; set; }

        public string RuleCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public string Severity { get; set; } = "WARNING";
        public string Status { get; set; } = "OPEN";

        public string? FarmId { get; set; }
        public string? FieldId { get; set; }
        public string? ZoneId { get; set; }

        public string? SensorId { get; set; }
        public string? SensorCode { get; set; }

        public int SensorTypeId { get; set; }
        public string SensorTypeName { get; set; } = string.Empty;

        public double MetricValue { get; set; }
        public double? ThresholdValue { get; set; }

        public string? SourceName { get; set; }
        public DateTime OccurredAt { get; set; }

        public string PayloadJson { get; set; } = "{}";
    }
}