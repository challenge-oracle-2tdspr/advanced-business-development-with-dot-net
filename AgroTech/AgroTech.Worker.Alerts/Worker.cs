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

        private IConnection? _connection;
        private IChannel? _channel;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public Worker(
            ILogger<Worker> logger,
            IOptions<RabbitMqConsumerOptions> options)
        {
            _logger = logger;
            _options = options.Value;
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

                    ProcessAlert(sensorEvent);

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

        private void ProcessAlert(SensorReadingCreatedEvent sensorEvent)
        {
            string? message = null;
            string severity = "Info";

            switch (sensorEvent.SensorType)
            {
                case 13: // Umidade do Solo
                    if (sensorEvent.Value < 25)
                    {
                        severity = "High";
                        message = $"Umidade do solo crítica ({sensorEvent.Value}). Avaliar aumento de irrigação.";
                    }
                    else if (sensorEvent.Value < 40)
                    {
                        severity = "Medium";
                        message = $"Umidade do solo moderada ({sensorEvent.Value}). Monitorar irrigação.";
                    }
                    break;

                case 17: // Chuva
                    if (sensorEvent.Value > 0)
                    {
                        severity = "High";
                        message = $"Chuva detectada ({sensorEvent.Value}). Avaliar suspensão da irrigação.";
                    }
                    break;

                case 16: // Velocidade do Vento
                    if (sensorEvent.Value > 20)
                    {
                        severity = "High";
                        message = $"Vento elevado ({sensorEvent.Value}). Evitar pulverização.";
                    }
                    break;

                case 14: // pH do Solo
                    if (sensorEvent.Value < 5.5 || sensorEvent.Value > 7.5)
                    {
                        severity = "High";
                        message = $"pH do solo fora da faixa ideal ({sensorEvent.Value}). Avaliar correção do solo.";
                    }
                    break;

                case 11: // Temperatura do Ar
                    if (sensorEvent.Value > 35)
                    {
                        severity = "High";
                        message = $"Temperatura do ar elevada ({sensorEvent.Value}). Possível estresse térmico.";
                    }
                    break;
            }

            if (message is null)
            {
                _logger.LogInformation(
                    "Sem alerta gerado. Sensor={SensorName}, Tipo={SensorType}, Valor={Value}, CorrelationId={CorrelationId}",
                    sensorEvent.SensorName,
                    sensorEvent.SensorType,
                    sensorEvent.Value,
                    sensorEvent.CorrelationId);

                return;
            }

            _logger.LogWarning(
                "ALERTA [{Severity}] {Message} Sensor={SensorName}, Tipo={SensorType}, CorrelationId={CorrelationId}",
                severity,
                message,
                sensorEvent.SensorName,
                sensorEvent.SensorType,
                sensorEvent.CorrelationId);
        }

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