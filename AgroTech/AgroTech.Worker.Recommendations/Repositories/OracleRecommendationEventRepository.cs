using System.Data;
using AgroTech.Worker.Recommendations.Configuration;
using AgroTech.Worker.Recommendations.Models;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;

namespace AgroTech.Worker.Recommendations.Repositories
{
    public class OracleRecommendationEventRepository : IRecommendationEventRepository
    {
        private readonly string _connectionString;

        public OracleRecommendationEventRepository(IOptions<OracleDatabaseOptions> options)
        {
            _connectionString = options.Value.ConnectionString;

            if (string.IsNullOrWhiteSpace(_connectionString))
                throw new InvalidOperationException("OracleDatabase:ConnectionString não configurada.");
        }

        public async Task SaveAsync(RecommendationEventRecord recommendation, CancellationToken cancellationToken)
        {
            const string sql = @"
merge into agt_recommendation_event dst
using (
    select :event_key as event_key
    from dual
) src
on (dst.event_key = src.event_key)
when not matched then
insert (
    event_key,
    reading_id,
    correlation_id,
    related_alert_event_key,
    rule_code,
    title,
    recommendation_text,
    priority,
    status,
    category,
    action_type,
    suggested_value,
    farm_id,
    field_id,
    zone_id,
    sensor_id,
    sensor_code,
    sensor_type_id,
    sensor_type_name,
    metric_value,
    source_name,
    occurred_at,
    expires_at,
    payload_json
)
values (
    :event_key,
    :reading_id,
    :correlation_id,
    :related_alert_event_key,
    :rule_code,
    :title,
    :recommendation_text,
    :priority,
    :status,
    :category,
    :action_type,
    :suggested_value,
    :farm_id,
    :field_id,
    :zone_id,
    :sensor_id,
    :sensor_code,
    :sensor_type_id,
    :sensor_type_name,
    :metric_value,
    :source_name,
    :occurred_at,
    :expires_at,
    :payload_json
)";

            await using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new OracleCommand(sql, connection)
            {
                BindByName = true,
                CommandType = CommandType.Text
            };

            command.Parameters.Add("event_key", OracleDbType.Varchar2).Value = recommendation.EventKey;
            command.Parameters.Add("reading_id", OracleDbType.Varchar2).Value = recommendation.ReadingId;
            command.Parameters.Add("correlation_id", OracleDbType.Varchar2).Value = (object?)recommendation.CorrelationId ?? DBNull.Value;
            command.Parameters.Add("related_alert_event_key", OracleDbType.Varchar2).Value = (object?)recommendation.RelatedAlertEventKey ?? DBNull.Value;
            command.Parameters.Add("rule_code", OracleDbType.Varchar2).Value = recommendation.RuleCode;
            command.Parameters.Add("title", OracleDbType.Varchar2).Value = recommendation.Title;
            command.Parameters.Add("recommendation_text", OracleDbType.Varchar2).Value = recommendation.RecommendationText;
            command.Parameters.Add("priority", OracleDbType.Varchar2).Value = recommendation.Priority;
            command.Parameters.Add("status", OracleDbType.Varchar2).Value = recommendation.Status;
            command.Parameters.Add("category", OracleDbType.Varchar2).Value = (object?)recommendation.Category ?? DBNull.Value;
            command.Parameters.Add("action_type", OracleDbType.Varchar2).Value = (object?)recommendation.ActionType ?? DBNull.Value;
            command.Parameters.Add("suggested_value", OracleDbType.Varchar2).Value = (object?)recommendation.SuggestedValue ?? DBNull.Value;
            command.Parameters.Add("farm_id", OracleDbType.Varchar2).Value = (object?)recommendation.FarmId ?? DBNull.Value;
            command.Parameters.Add("field_id", OracleDbType.Varchar2).Value = (object?)recommendation.FieldId ?? DBNull.Value;
            command.Parameters.Add("zone_id", OracleDbType.Varchar2).Value = (object?)recommendation.ZoneId ?? DBNull.Value;
            command.Parameters.Add("sensor_id", OracleDbType.Varchar2).Value = (object?)recommendation.SensorId ?? DBNull.Value;
            command.Parameters.Add("sensor_code", OracleDbType.Varchar2).Value = (object?)recommendation.SensorCode ?? DBNull.Value;
            command.Parameters.Add("sensor_type_id", OracleDbType.Int32).Value = recommendation.SensorTypeId;
            command.Parameters.Add("sensor_type_name", OracleDbType.Varchar2).Value = recommendation.SensorTypeName;
            command.Parameters.Add("metric_value", OracleDbType.Double).Value = recommendation.MetricValue;
            command.Parameters.Add("source_name", OracleDbType.Varchar2).Value = (object?)recommendation.SourceName ?? DBNull.Value;
            command.Parameters.Add("occurred_at", OracleDbType.TimeStamp).Value = recommendation.OccurredAt;
            command.Parameters.Add("expires_at", OracleDbType.TimeStamp).Value = (object?)recommendation.ExpiresAt ?? DBNull.Value;
            command.Parameters.Add("payload_json", OracleDbType.Clob).Value = recommendation.PayloadJson;

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}