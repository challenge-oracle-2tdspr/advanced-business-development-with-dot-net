using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AgroTech.Infrastructure.Mongo.Documents
{
    public class SensorReadingDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid SensorId { get; set; }

        public string SensorName { get; set; } = string.Empty;
        public string SensorType { get; set; } = string.Empty;
        public double Value { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Timestamp { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Source { get; set; } = "api";
        public string CorrelationId { get; set; } = string.Empty;
    }
}