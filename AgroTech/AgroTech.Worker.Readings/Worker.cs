using System.Text;
using System.Text.Json;
using AgroTech.Contracts.Events;
using AgroTech.Worker.Readings.Configuration;
using AgroTech.Worker.Readings.Models;
using AgroTech.Worker.Readings.Repositories;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AgroTech.Worker.Readings
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly RabbitMqConsumerOptions _options;
        private readonly ISensorReadingEventRepository _repository;

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
            ISensorReadingEventRepository repository)
        {
            _logger = logger;
            _options = options.Value;
            _repository = repository;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_options.Enabled)
            {
                _logger.LogWarning("Worker de readings iniciado com RabbitMQ desabilitado.");
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
                "Worker de readings conectado ao RabbitMQ. Queue={Queue}, Exchange={Exchange}, RoutingKey={RoutingKey}",
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

                    var record = BuildRecord(sensorEvent, json);
                    await _repository.SaveAsync(record, stoppingToken);

                    _logger.LogInformation(
                        "READING [{SensorType}] {SensorName}={Value} CorrelationId={CorrelationId} EventKey={EventKey}",
                        sensorEvent.SensorType,
                        sensorEvent.SensorName,
                        sensorEvent.Value,
                        sensorEvent.CorrelationId,
                        record.EventKey);

                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem da fila de readings.");
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

        private static SensorReadingEventRecord BuildRecord(SensorReadingCreatedEvent sensorEvent, string originalJson)
        {
            var readingId = BuildReadingId(sensorEvent);
            var eventKey = $"READING:{readingId}";

            return new SensorReadingEventRecord
            {
                EventKey = eventKey,
                ReadingId = readingId,
                CorrelationId = NullIfEmpty(sensorEvent.CorrelationId),
                SensorId = sensorEvent.SensorId.ToString(),
                SensorCode = NullIfEmpty(sensorEvent.SensorName),
                SensorTypeId = sensorEvent.SensorType,
                SensorTypeName = GetSensorTypeName(sensorEvent.SensorType, sensorEvent.SensorName),
                MetricValue = sensorEvent.Value,
                SourceName = NullIfEmpty(sensorEvent.Source),
                FarmId = NullIfEmpty(sensorEvent.FarmId),
                FieldId = NullIfEmpty(sensorEvent.FieldId),
                ZoneId = NullIfEmpty(sensorEvent.ZoneId),
                OccurredAt = sensorEvent.Timestamp,
                PayloadJson = originalJson
            };
        }

        private static string BuildReadingId(SensorReadingCreatedEvent sensorEvent)
        {
            if (!string.IsNullOrWhiteSpace(sensorEvent.CorrelationId))
                return $"{sensorEvent.CorrelationId}:{sensorEvent.SensorType}";

            return $"{sensorEvent.SensorId:N}:{sensorEvent.SensorType}:{sensorEvent.Timestamp:O}";
        }

        private static string GetSensorTypeName(int sensorType, string? fallbackName) =>
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
                _ => string.IsNullOrWhiteSpace(fallbackName) ? "Desconhecido" : fallbackName
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