using AgroTech.Worker.Alerts.Models;
using AgroTech.Worker.Alerts.Repositories;
using System.Text;
using System.Text.Json;
using AgroTech.Contracts.Events;
using AgroTech.Worker.Alerts.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AgroTech.Worker.Alerts
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly RabbitMqConsumerOptions _options;
        private readonly IAlertEventRepository _alertRepository;

        private IConnection? _connection;
        private IChannel? _channel;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public Worker(
            ILogger<Worker> logger,
            IOptions<RabbitMqConsumerOptions> options,
            IAlertEventRepository alertRepository)
        {
            _logger = logger;
            _options = options.Value;
            _alertRepository = alertRepository;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_options.Enabled)
            {
                _logger.LogWarning("Worker de alertas iniciado com RabbitMQ desabilitado.");
                await base.StartAsync(cancellationToken);
                return;
            }

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                ClientProvidedName = _options.ClientProvidedName,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await _channel.ExchangeDeclareAsync(
                exchange: _options.Exchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: _options.Queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            await _channel.QueueBindAsync(
                queue: _options.Queue,
                exchange: _options.Exchange,
                routingKey: _options.RoutingKey,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Worker de alertas conectado ao RabbitMQ. Queue={Queue}, Exchange={Exchange}, RoutingKey={RoutingKey}",
                _options.Queue,
                _options.Exchange,
                _options.RoutingKey);

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_options.Enabled)
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }

                return;
            }

            if (_channel is null)
                throw new InvalidOperationException("Canal RabbitMQ não foi inicializado.");

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);

                    var sensorEvent = JsonSerializer.Deserialize<SensorReadingCreatedEvent>(json, JsonOptions);

                    if (sensorEvent is null)
                    {
                        _logger.LogWarning("Mensagem recebida não pôde ser desserializada.");
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                        return;
                    }

                    await ProcessAlertAsync(sensorEvent, json, stoppingToken);

                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem da fila de alertas.");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: _options.Queue,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task ProcessAlertAsync(
            SensorReadingCreatedEvent sensorEvent,
            string originalJson,
            CancellationToken cancellationToken)
        {
            var alertRecord = BuildAlertRecord(sensorEvent, originalJson);

            if (alertRecord is null)
            {
                _logger.LogInformation(
                    "Sem alerta gerado. Sensor={SensorName}, Tipo={SensorType}, Valor={Value}, CorrelationId={CorrelationId}",
                    sensorEvent.SensorName,
                    sensorEvent.SensorType,
                    sensorEvent.Value,
                    sensorEvent.CorrelationId);

                return;
            }

            await _alertRepository.SaveAsync(alertRecord, cancellationToken);

            _logger.LogWarning(
                "ALERTA [{Severity}] {Message} Sensor={SensorName}, Tipo={SensorType}, CorrelationId={CorrelationId}, EventKey={EventKey}",
                alertRecord.Severity,
                alertRecord.Message,
                sensorEvent.SensorName,
                sensorEvent.SensorType,
                sensorEvent.CorrelationId,
                alertRecord.EventKey);
        }

        private static AlertEventRecord? BuildAlertRecord(SensorReadingCreatedEvent sensorEvent, string originalJson)
        {
            string? ruleCode = null;
            string? title = null;
            string? message = null;
            string severity = "INFO";
            double? thresholdValue = null;

            switch (sensorEvent.SensorType)
            {
                case 13: // Umidade do Solo
                    if (sensorEvent.Value < 25)
                    {
                        ruleCode = "SOIL_MOISTURE_LOW_CRITICAL";
                        title = "Umidade do solo crítica";
                        severity = "CRITICAL";
                        thresholdValue = 25;
                        message = $"Umidade do solo crítica ({sensorEvent.Value}). Avaliar aumento de irrigação.";
                    }
                    else if (sensorEvent.Value < 40)
                    {
                        ruleCode = "SOIL_MOISTURE_LOW_WARNING";
                        title = "Umidade do solo baixa";
                        severity = "WARNING";
                        thresholdValue = 40;
                        message = $"Umidade do solo moderada ({sensorEvent.Value}). Monitorar irrigação.";
                    }
                    break;

                case 17: // Chuva
                    if (sensorEvent.Value > 0)
                    {
                        ruleCode = "RAIN_DETECTED";
                        title = "Chuva detectada";
                        severity = "WARNING";
                        thresholdValue = 0;
                        message = $"Chuva detectada ({sensorEvent.Value}). Avaliar suspensão da irrigação.";
                    }
                    break;

                case 16: // Velocidade do Vento
                    if (sensorEvent.Value > 20)
                    {
                        ruleCode = "HIGH_WIND";
                        title = "Vento elevado";
                        severity = "WARNING";
                        thresholdValue = 20;
                        message = $"Vento elevado ({sensorEvent.Value}). Evitar pulverização.";
                    }
                    break;

                case 14: // pH do Solo
                    if (sensorEvent.Value < 5.5 || sensorEvent.Value > 7.5)
                    {
                        ruleCode = "PH_OUT_OF_RANGE";
                        title = "pH do solo fora da faixa ideal";
                        severity = "WARNING";
                        thresholdValue = sensorEvent.Value < 5.5 ? 5.5 : 7.5;
                        message = $"pH do solo fora da faixa ideal ({sensorEvent.Value}). Avaliar correção do solo.";
                    }
                    break;

                case 11: // Temperatura do Ar
                    if (sensorEvent.Value > 35)
                    {
                        ruleCode = "AIR_TEMP_HIGH";
                        title = "Temperatura do ar elevada";
                        severity = "WARNING";
                        thresholdValue = 35;
                        message = $"Temperatura do ar elevada ({sensorEvent.Value}). Possível estresse térmico.";
                    }
                    break;
            }

            if (message is null || ruleCode is null || title is null)
                return null;

            var readingId = BuildReadingId(sensorEvent);
            var eventKey = $"ALERT:{readingId}:{ruleCode}";

            return new AlertEventRecord
            {
                EventKey = eventKey,
                ReadingId = readingId,
                CorrelationId = NullIfEmpty(sensorEvent.CorrelationId),
                RuleCode = ruleCode,
                Title = title,
                Message = message,
                Severity = severity,
                Status = "OPEN",
                FarmId = NullIfEmpty(sensorEvent.FarmId),
                FieldId = NullIfEmpty(sensorEvent.FieldId),
                ZoneId = NullIfEmpty(sensorEvent.ZoneId),
                SensorId = sensorEvent.SensorId.ToString(),
                SensorCode = NullIfEmpty(sensorEvent.SensorName),
                SensorTypeId = sensorEvent.SensorType,
                SensorTypeName = GetSensorTypeName(sensorEvent.SensorType),
                MetricValue = sensorEvent.Value,
                ThresholdValue = thresholdValue,
                SourceName = NullIfEmpty(sensorEvent.Source),
                OccurredAt = sensorEvent.Timestamp,
                PayloadJson = originalJson
            };
        }

        private static string BuildReadingId(SensorReadingCreatedEvent sensorEvent)
        {
            if (!string.IsNullOrWhiteSpace(sensorEvent.CorrelationId))
                return sensorEvent.CorrelationId;

            return $"{sensorEvent.SensorId:N}:{sensorEvent.SensorType}:{sensorEvent.Timestamp:O}";
        }

        private static string GetSensorTypeName(int sensorType) =>
            sensorType switch
            {
                11 => "Temperatura do Ar",
                12 => "Umidade do Ar",
                13 => "Umidade do Solo",
                14 => "pH do Solo",
                15 => "Luminosidade",
                16 => "Velocidade do Vento",
                17 => "Chuva",
                18 => "Temperatura do Solo",
                _ => "Desconhecido"
            };

        private static string? NullIfEmpty(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value;

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel is not null)
            {
                try
                {
                    if (_channel.IsOpen)
                        await _channel.CloseAsync();
                }
                catch
                {
                }

                await _channel.DisposeAsync();
            }

            if (_connection is not null)
            {
                try
                {
                    if (_connection.IsOpen)
                        await _connection.CloseAsync();
                }
                catch
                {
                }

                await _connection.DisposeAsync();
            }

            await base.StopAsync(cancellationToken);
        }
    }
}