using System.Data;
using AgroTech.Worker.Readings.Configuration;
using AgroTech.Worker.Readings.Models;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;

namespace AgroTech.Worker.Readings.Repositories
{
    public class OracleSensorReadingEventRepository : ISensorReadingEventRepository
    {
        private readonly string _connectionString;

        public OracleSensorReadingEventRepository(IOptions<OracleDatabaseOptions> options)
        {
            _connectionString = options.Value.ConnectionString;

            if (string.IsNullOrWhiteSpace(_connectionString))
                throw new InvalidOperationException("OracleDatabase:ConnectionString não configurada.");
        }

        public async Task SaveAsync(SensorReadingEventRecord record, CancellationToken cancellationToken)
        {
            const string sql = @"
merge into agt_sensor_reading_event dst
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
    sensor_id,
    sensor_code,
    sensor_type_id,
    sensor_type_name,
    metric_value,
    source_name,
    farm_id,
    field_id,
    zone_id,
    occurred_at,
    payload_json
)
values (
    :event_key,
    :reading_id,
    :correlation_id,
    :sensor_id,
    :sensor_code,
    :sensor_type_id,
    :sensor_type_name,
    :metric_value,
    :source_name,
    :farm_id,
    :field_id,
    :zone_id,
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

            command.Parameters.Add("event_key", OracleDbType.Varchar2).Value = record.EventKey;
            command.Parameters.Add("reading_id", OracleDbType.Varchar2).Value = record.ReadingId;
            command.Parameters.Add("correlation_id", OracleDbType.Varchar2).Value = (object?)record.CorrelationId ?? DBNull.Value;
            command.Parameters.Add("sensor_id", OracleDbType.Varchar2).Value = (object?)record.SensorId ?? DBNull.Value;
            command.Parameters.Add("sensor_code", OracleDbType.Varchar2).Value = (object?)record.SensorCode ?? DBNull.Value;
            command.Parameters.Add("sensor_type_id", OracleDbType.Int32).Value = record.SensorTypeId;
            command.Parameters.Add("sensor_type_name", OracleDbType.Varchar2).Value = record.SensorTypeName;
            command.Parameters.Add("metric_value", OracleDbType.Double).Value = record.MetricValue;
            command.Parameters.Add("source_name", OracleDbType.Varchar2).Value = (object?)record.SourceName ?? DBNull.Value;
            command.Parameters.Add("farm_id", OracleDbType.Varchar2).Value = (object?)record.FarmId ?? DBNull.Value;
            command.Parameters.Add("field_id", OracleDbType.Varchar2).Value = (object?)record.FieldId ?? DBNull.Value;
            command.Parameters.Add("zone_id", OracleDbType.Varchar2).Value = (object?)record.ZoneId ?? DBNull.Value;
            command.Parameters.Add("occurred_at", OracleDbType.TimeStamp).Value = record.OccurredAt;
            command.Parameters.Add("payload_json", OracleDbType.Clob).Value = record.PayloadJson;

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
