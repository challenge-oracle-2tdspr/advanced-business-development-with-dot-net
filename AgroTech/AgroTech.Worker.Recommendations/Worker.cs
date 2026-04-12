using System.Text;
using System.Text.Json;
using AgroTech.Contracts.Events;
using AgroTech.Worker.Recommendations.Configuration;
using AgroTech.Worker.Recommendations.Models;
using AgroTech.Worker.Recommendations.Repositories;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AgroTech.Worker.Recommendations
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly RabbitMqConsumerOptions _options;
        private readonly IRecommendationEventRepository _recommendationRepository;

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
            IRecommendationEventRepository recommendationRepository)
        {
            _logger = logger;
            _options = options.Value;
            _recommendationRepository = recommendationRepository;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_options.Enabled)
            {
                _logger.LogWarning("Worker de recommendations iniciado com RabbitMQ desabilitado.");
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
                "Worker de recommendations conectado ao RabbitMQ. Queue={Queue}, Exchange={Exchange}, RoutingKey={RoutingKey}",
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

                    await ProcessRecommendationAsync(sensorEvent, json, stoppingToken);

                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem da fila de recommendations.");
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

        private async Task ProcessRecommendationAsync(
            SensorReadingCreatedEvent sensorEvent,
            string originalJson,
            CancellationToken cancellationToken)
        {
            var recommendationRecord = BuildRecommendationRecord(sensorEvent, originalJson);

            if (recommendationRecord is null)
            {
                _logger.LogInformation(
                    "Sem recomendação gerada. Sensor={SensorName}, Tipo={SensorType}, Valor={Value}, CorrelationId={CorrelationId}",
                    sensorEvent.SensorName,
                    sensorEvent.SensorType,
                    sensorEvent.Value,
                    sensorEvent.CorrelationId);

                return;
            }

            await _recommendationRepository.SaveAsync(recommendationRecord, cancellationToken);

            _logger.LogInformation(
                "RECOMMENDATION [{Category}] {Recommendation} Sensor={SensorName}, Tipo={SensorType}, CorrelationId={CorrelationId}, EventKey={EventKey}",
                recommendationRecord.Category,
                recommendationRecord.RecommendationText,
                sensorEvent.SensorName,
                sensorEvent.SensorType,
                sensorEvent.CorrelationId,
                recommendationRecord.EventKey);
        }

        private static RecommendationEventRecord? BuildRecommendationRecord(
            SensorReadingCreatedEvent sensorEvent,
            string originalJson)
        {
            string? ruleCode = null;
            string? title = null;
            string? recommendation = null;
            string category = "Monitoramento";
            string priority = "MEDIUM";
            string? actionType = null;
            string? suggestedValue = null;
            string? relatedAlertEventKey = null;
            DateTime? expiresAt = null;

            var readingId = BuildReadingId(sensorEvent);

            switch (sensorEvent.SensorType)
            {
                case 13: // Umidade do Solo
                    if (sensorEvent.Value < 25)
                    {
                        category = "Irrigação";
                        priority = "HIGH";
                        actionType = "INCREASE_IRRIGATION";
                        suggestedValue = "15%";
                        ruleCode = "SOIL_MOISTURE_INCREASE_IRRIGATION";
                        title = "Aumentar irrigação";
                        recommendation = $"Aumentar irrigação em 15%. Umidade do solo em {sensorEvent.Value}.";
                        relatedAlertEventKey = $"ALERT:{readingId}:SOIL_MOISTURE_LOW_CRITICAL";
                    }
                    else if (sensorEvent.Value < 40)
                    {
                        category = "Irrigação";
                        priority = "MEDIUM";
                        actionType = "REVIEW_IRRIGATION_PLAN";
                        suggestedValue = "Revisar plano";
                        ruleCode = "SOIL_MOISTURE_REVIEW_IRRIGATION";
                        title = "Revisar irrigação";
                        recommendation = $"Revisar plano de irrigação. Umidade do solo em {sensorEvent.Value}.";
                        relatedAlertEventKey = $"ALERT:{readingId}:SOIL_MOISTURE_LOW_WARNING";
                    }
                    break;

                case 17: // Chuva
                    if (sensorEvent.Value > 0)
                    {
                        category = "Irrigação";
                        priority = "HIGH";
                        actionType = "SUSPEND_IRRIGATION";
                        suggestedValue = "12h";
                        ruleCode = "RAIN_SUSPEND_IRRIGATION";
                        title = "Suspender irrigação temporariamente";
                        recommendation = $"Suspender irrigação temporariamente. Chuva detectada: {sensorEvent.Value}.";
                        relatedAlertEventKey = $"ALERT:{readingId}:RAIN_DETECTED";
                        expiresAt = sensorEvent.Timestamp.AddHours(12);
                    }
                    break;

                case 16: // Velocidade do Vento
                    if (sensorEvent.Value > 20)
                    {
                        category = "Pulverização";
                        priority = "HIGH";
                        actionType = "POSTPONE_SPRAY";
                        suggestedValue = "Adiar aplicação";
                        ruleCode = "HIGH_WIND_POSTPONE_SPRAY";
                        title = "Adiar pulverização";
                        recommendation = $"Adiar pulverização. Velocidade do vento em {sensorEvent.Value}.";
                        relatedAlertEventKey = $"ALERT:{readingId}:HIGH_WIND";
                    }
                    break;

                case 14: // pH do Solo
                    if (sensorEvent.Value < 5.5)
                    {
                        category = "Solo";
                        priority = "HIGH";
                        actionType = "SOIL_CORRECTION";
                        suggestedValue = "Calagem";
                        ruleCode = "LOW_PH_SOIL_CORRECTION";
                        title = "Avaliar correção com calagem";
                        recommendation = $"Avaliar correção com calagem. pH do solo em {sensorEvent.Value}.";
                        relatedAlertEventKey = $"ALERT:{readingId}:PH_OUT_OF_RANGE";
                    }
                    else if (sensorEvent.Value > 7.5)
                    {
                        category = "Solo";
                        priority = "HIGH";
                        actionType = "SOIL_CORRECTION";
                        suggestedValue = "Revisar alcalinidade";
                        ruleCode = "HIGH_PH_REVIEW_MANAGEMENT";
                        title = "Revisar manejo de alcalinidade";
                        recommendation = $"Avaliar manejo para redução da alcalinidade. pH do solo em {sensorEvent.Value}.";
                        relatedAlertEventKey = $"ALERT:{readingId}:PH_OUT_OF_RANGE";
                    }
                    break;

                case 11: // Temperatura do Ar
                    if (sensorEvent.Value > 35)
                    {
                        category = "Clima";
                        priority = "MEDIUM";
                        actionType = "REINFORCE_MONITORING";
                        suggestedValue = "Monitoramento hídrico e térmico";
                        ruleCode = "AIR_TEMP_REINFORCE_MONITORING";
                        title = "Reforçar monitoramento hídrico";
                        recommendation = $"Reforçar monitoramento hídrico e térmico. Temperatura do ar em {sensorEvent.Value}.";
                        relatedAlertEventKey = $"ALERT:{readingId}:AIR_TEMP_HIGH";
                    }
                    break;

                case 18: // Temperatura do Solo
                    if (sensorEvent.Value > 30)
                    {
                        category = "Solo";
                        priority = "MEDIUM";
                        actionType = "REVIEW_WATER_MANAGEMENT";
                        suggestedValue = "Revisar manejo hídrico";
                        ruleCode = "SOIL_TEMP_REVIEW_WATER_MANAGEMENT";
                        title = "Monitorar aquecimento do solo";
                        recommendation = $"Monitorar aquecimento do solo e revisar manejo hídrico. Temperatura do solo em {sensorEvent.Value}.";
                    }
                    break;

                case 15: // Luminosidade
                    if (sensorEvent.Value > 900)
                    {
                        category = "Clima";
                        priority = "MEDIUM";
                        actionType = "REVIEW_WATER_MANAGEMENT";
                        suggestedValue = "Avaliar manejo hídrico";
                        ruleCode = "HIGH_LIGHT_REVIEW_WATER_MANAGEMENT";
                        title = "Avaliar impacto de alta luminosidade";
                        recommendation = $"Avaliar impacto de alta luminosidade no manejo hídrico. Luminosidade em {sensorEvent.Value}.";
                    }
                    break;
            }

            if (recommendation is null || ruleCode is null || title is null)
                return null;

            var eventKey = $"RECO:{readingId}:{ruleCode}";

            return new RecommendationEventRecord
            {
                EventKey = eventKey,
                ReadingId = readingId,
                CorrelationId = NullIfEmpty(sensorEvent.CorrelationId),
                RelatedAlertEventKey = relatedAlertEventKey,
                RuleCode = ruleCode,
                Title = title,
                RecommendationText = recommendation,
                Priority = priority,
                Status = "OPEN",
                Category = category,
                ActionType = actionType,
                SuggestedValue = suggestedValue,
                FarmId = NullIfEmpty(sensorEvent.FarmId),
                FieldId = NullIfEmpty(sensorEvent.FieldId),
                ZoneId = NullIfEmpty(sensorEvent.ZoneId),
                SensorId = sensorEvent.SensorId.ToString(),
                SensorCode = NullIfEmpty(sensorEvent.SensorName),
                SensorTypeId = sensorEvent.SensorType,
                SensorTypeName = GetSensorTypeName(sensorEvent.SensorType),
                MetricValue = sensorEvent.Value,
                SourceName = NullIfEmpty(sensorEvent.Source),
                OccurredAt = sensorEvent.Timestamp,
                ExpiresAt = expiresAt,
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