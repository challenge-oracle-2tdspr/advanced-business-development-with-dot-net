using AgroTech.Worker.Alerts.Configuration;
using AgroTech.Worker.Alerts.Models;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace AgroTech.Worker.Alerts.Repositories
{
    public class OracleAlertEventRepository : IAlertEventRepository
    {
        private readonly string _connectionString;

        public OracleAlertEventRepository(IOptions<OracleDatabaseOptions> options)
        {
            _connectionString = options.Value.ConnectionString;

            if (string.IsNullOrWhiteSpace(_connectionString))
                throw new InvalidOperationException("OracleDatabase:ConnectionString não configurada.");
        }

        public async Task SaveAsync(AlertEventRecord alert, CancellationToken cancellationToken)
        {
            const string sql = @"
merge into agt_alert_event dst
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
    rule_code,
    title,
    message,
    severity,
    status,
    farm_id,
    field_id,
    zone_id,
    sensor_id,
    sensor_code,
    sensor_type_id,
    sensor_type_name,
    metric_value,
    threshold_value,
    source_name,
    occurred_at,
    payload_json
)
values (
    :event_key,
    :reading_id,
    :correlation_id,
    :rule_code,
    :title,
    :message,
    :severity,
    :status,
    :farm_id,
    :field_id,
    :zone_id,
    :sensor_id,
    :sensor_code,
    :sensor_type_id,
    :sensor_type_name,
    :metric_value,
    :threshold_value,
    :source_name,
    :occurred_at,
    :payload_json
)";

            await using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new OracleCommand(sql, connection)
            {
                BindByName = true,
                CommandType = CommandType.Text
            };

            command.Parameters.Add("event_key", OracleDbType.Varchar2).Value = alert.EventKey;
            command.Parameters.Add("reading_id", OracleDbType.Varchar2).Value = alert.ReadingId;
            command.Parameters.Add("correlation_id", OracleDbType.Varchar2).Value = (object?)alert.CorrelationId ?? DBNull.Value;
            command.Parameters.Add("rule_code", OracleDbType.Varchar2).Value = alert.RuleCode;
            command.Parameters.Add("title", OracleDbType.Varchar2).Value = alert.Title;
            command.Parameters.Add("message", OracleDbType.Varchar2).Value = alert.Message;
            command.Parameters.Add("severity", OracleDbType.Varchar2).Value = alert.Severity;
            command.Parameters.Add("status", OracleDbType.Varchar2).Value = alert.Status;
            command.Parameters.Add("farm_id", OracleDbType.Varchar2).Value = (object?)alert.FarmId ?? DBNull.Value;
            command.Parameters.Add("field_id", OracleDbType.Varchar2).Value = (object?)alert.FieldId ?? DBNull.Value;
            command.Parameters.Add("zone_id", OracleDbType.Varchar2).Value = (object?)alert.ZoneId ?? DBNull.Value;
            command.Parameters.Add("sensor_id", OracleDbType.Varchar2).Value = (object?)alert.SensorId ?? DBNull.Value;
            command.Parameters.Add("sensor_code", OracleDbType.Varchar2).Value = (object?)alert.SensorCode ?? DBNull.Value;
            command.Parameters.Add("sensor_type_id", OracleDbType.Int32).Value = alert.SensorTypeId;
            command.Parameters.Add("sensor_type_name", OracleDbType.Varchar2).Value = alert.SensorTypeName;
            command.Parameters.Add("metric_value", OracleDbType.Double).Value = alert.MetricValue;
            command.Parameters.Add("threshold_value", OracleDbType.Double).Value = (object?)alert.ThresholdValue ?? DBNull.Value;
            command.Parameters.Add("source_name", OracleDbType.Varchar2).Value = (object?)alert.SourceName ?? DBNull.Value;
            command.Parameters.Add("occurred_at", OracleDbType.TimeStamp).Value = alert.OccurredAt;
            command.Parameters.Add("payload_json", OracleDbType.Clob).Value = alert.PayloadJson;

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}