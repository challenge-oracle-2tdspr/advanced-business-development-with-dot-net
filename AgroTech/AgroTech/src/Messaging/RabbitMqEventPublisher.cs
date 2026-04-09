using System.Text.Json;
using AgroTech.Configuration;
using AgroTech.Contracts.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace AgroTech.Messaging
{
    public class RabbitMqEventPublisher : IEventPublisher, IAsyncDisposable
    {
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqEventPublisher> _logger;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private IConnection? _connection;
        private IChannel? _channel;
        private bool _topologyDeclared;

        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public RabbitMqEventPublisher(
            IOptions<RabbitMqOptions> options,
            ILogger<RabbitMqEventPublisher> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task PublishSensorReadingCreatedAsync(
            SensorReadingCreatedEvent @event,
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
            {
                _logger.LogDebug(
                    "RabbitMQ está desabilitado. Evento {EventName} não será publicado.",
                    @event.EventName);

                return;
            }

            try
            {
                var connected = await EnsureConnectionAsync(cancellationToken);

                if (!connected || _channel is null)
                {
                    _logger.LogWarning(
                        "RabbitMQ indisponível. Evento {EventName} do sensor {SensorId} não foi publicado.",
                        @event.EventName,
                        @event.SensorId);

                    return;
                }

                var body = JsonSerializer.SerializeToUtf8Bytes(@event, JsonSerializerOptions);

                var properties = new BasicProperties
                {
                    ContentType = "application/json",
                    DeliveryMode = DeliveryModes.Persistent,
                    CorrelationId = @event.CorrelationId,
                    MessageId = Guid.NewGuid().ToString(),
                    Type = @event.EventName,
                    AppId = "AgroTech.Api"
                };

                await _channel.BasicPublishAsync(
                    exchange: _options.Exchange,
                    routingKey: _options.SensorReadingCreatedRoutingKey,
                    mandatory: false,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation(
                    "Evento {EventName} publicado com sucesso. SensorId={SensorId}, CorrelationId={CorrelationId}",
                    @event.EventName,
                    @event.SensorId,
                    @event.CorrelationId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Falha ao publicar evento {EventName} do sensor {SensorId}. O fluxo principal continuará.",
                    @event.EventName,
                    @event.SensorId);
            }
        }

        private async Task<bool> EnsureConnectionAsync(CancellationToken cancellationToken)
        {
            if (_connection is { IsOpen: true } &&
                _channel is { IsOpen: true } &&
                _topologyDeclared)
            {
                return true;
            }

            await _semaphore.WaitAsync(cancellationToken);

            try
            {
                if (_connection is { IsOpen: true } &&
                    _channel is { IsOpen: true } &&
                    _topologyDeclared)
                {
                    return true;
                }

                await DisposeRabbitMqObjectsAsync();

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

                await DeclareTopologyAsync(_channel, cancellationToken);

                _topologyDeclared = true;

                _logger.LogInformation(
                    "Conexão com RabbitMQ estabelecida em {Host}:{Port}. Exchange {Exchange} pronta.",
                    _options.HostName,
                    _options.Port,
                    _options.Exchange);

                return true;
            }
            catch (Exception ex)
            {
                _topologyDeclared = false;

                _logger.LogWarning(
                    ex,
                    "Não foi possível conectar ao RabbitMQ em {Host}:{Port}.",
                    _options.HostName,
                    _options.Port);

                return false;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task DeclareTopologyAsync(IChannel channel, CancellationToken cancellationToken)
        {
            await channel.ExchangeDeclareAsync(
                exchange: _options.Exchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                queue: _options.AlertsQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                queue: _options.RecommendationsQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            await channel.QueueBindAsync(
                queue: _options.AlertsQueue,
                exchange: _options.Exchange,
                routingKey: _options.SensorReadingCreatedRoutingKey,
                arguments: null,
                cancellationToken: cancellationToken);

            await channel.QueueBindAsync(
                queue: _options.RecommendationsQueue,
                exchange: _options.Exchange,
                routingKey: _options.SensorReadingCreatedRoutingKey,
                arguments: null,
                cancellationToken: cancellationToken);
        }

        private async Task DisposeRabbitMqObjectsAsync()
        {
            if (_channel is not null)
            {
                try
                {
                    if (_channel.IsOpen)
                    {
                        await _channel.CloseAsync();
                    }
                }
                catch
                {
                }

                await _channel.DisposeAsync();
                _channel = null;
            }

            if (_connection is not null)
            {
                try
                {
                    if (_connection.IsOpen)
                    {
                        await _connection.CloseAsync();
                    }
                }
                catch
                {
                }

                await _connection.DisposeAsync();
                _connection = null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeRabbitMqObjectsAsync();
            _semaphore.Dispose();
        }
    }
}