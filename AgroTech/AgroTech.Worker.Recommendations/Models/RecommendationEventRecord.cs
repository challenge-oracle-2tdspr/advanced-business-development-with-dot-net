namespace AgroTech.Worker.Recommendations.Models
{
    public class RecommendationEventRecord
    {
        public string EventKey { get; set; } = string.Empty;
        public string ReadingId { get; set; } = string.Empty;
        public string? CorrelationId { get; set; }
        public string? RelatedAlertEventKey { get; set; }

        public string RuleCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string RecommendationText { get; set; } = string.Empty;

        public string Priority { get; set; } = "MEDIUM";
        public string Status { get; set; } = "OPEN";

        public string? Category { get; set; }
        public string? ActionType { get; set; }
        public string? SuggestedValue { get; set; }

        public string? FarmId { get; set; }
        public string? FieldId { get; set; }
        public string? ZoneId { get; set; }

        public string? SensorId { get; set; }
        public string? SensorCode { get; set; }

        public int SensorTypeId { get; set; }
        public string SensorTypeName { get; set; } = string.Empty;

        public double MetricValue { get; set; }
        public string? SourceName { get; set; }

        public DateTime OccurredAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public string PayloadJson { get; set; } = "{}";
    }
}