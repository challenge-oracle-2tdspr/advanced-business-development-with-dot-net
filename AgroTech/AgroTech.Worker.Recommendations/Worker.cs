using System.Text;
using System.Text.Json;
using AgroTech.Contracts.Events;
using AgroTech.Worker.Recommendations.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AgroTech.Worker.Recommendations
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

                    ProcessRecommendation(sensorEvent);

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

        private void ProcessRecommendation(SensorReadingCreatedEvent sensorEvent)
        {
            string? recommendation = null;
            string category = "Monitoramento";

            switch (sensorEvent.SensorType)
            {
                case 13: // Umidade do Solo
                    if (sensorEvent.Value < 25)
                    {
                        category = "Irrigação";
                        recommendation = $"Aumentar irrigação em 15%. Umidade do solo em {sensorEvent.Value}.";
                    }
                    else if (sensorEvent.Value < 40)
                    {
                        category = "Irrigação";
                        recommendation = $"Revisar plano de irrigação. Umidade do solo em {sensorEvent.Value}.";
                    }
                    break;

                case 17: // Chuva
                    if (sensorEvent.Value > 0)
                    {
                        category = "Irrigação";
                        recommendation = $"Suspender irrigação temporariamente. Chuva detectada: {sensorEvent.Value}.";
                    }
                    break;

                case 16: // Velocidade do Vento
                    if (sensorEvent.Value > 20)
                    {
                        category = "Pulverização";
                        recommendation = $"Adiar pulverização. Velocidade do vento em {sensorEvent.Value}.";
                    }
                    break;

                case 14: // pH do Solo
                    if (sensorEvent.Value < 5.5)
                    {
                        category = "Solo";
                        recommendation = $"Avaliar correção com calagem. pH do solo em {sensorEvent.Value}.";
                    }
                    else if (sensorEvent.Value > 7.5)
                    {
                        category = "Solo";
                        recommendation = $"Avaliar manejo para redução da alcalinidade. pH do solo em {sensorEvent.Value}.";
                    }
                    break;

                case 11: // Temperatura do Ar
                    if (sensorEvent.Value > 35)
                    {
                        category = "Clima";
                        recommendation = $"Reforçar monitoramento hídrico e térmico. Temperatura do ar em {sensorEvent.Value}.";
                    }
                    break;

                case 18: // Temperatura do Solo
                    if (sensorEvent.Value > 30)
                    {
                        category = "Solo";
                        recommendation = $"Monitorar aquecimento do solo e revisar manejo hídrico. Temperatura do solo em {sensorEvent.Value}.";
                    }
                    break;

                case 15: // Luminosidade
                    if (sensorEvent.Value > 900)
                    {
                        category = "Clima";
                        recommendation = $"Avaliar impacto de alta luminosidade no manejo hídrico. Luminosidade em {sensorEvent.Value}.";
                    }
                    break;
            }

            if (recommendation is null)
            {
                _logger.LogInformation(
                    "Sem recomendação gerada. Sensor={SensorName}, Tipo={SensorType}, Valor={Value}, CorrelationId={CorrelationId}",
                    sensorEvent.SensorName,
                    sensorEvent.SensorType,
                    sensorEvent.Value,
                    sensorEvent.CorrelationId);

                return;
            }

            _logger.LogInformation(
                "RECOMMENDATION [{Category}] {Recommendation} Sensor={SensorName}, Tipo={SensorType}, CorrelationId={CorrelationId}",
                category,
                recommendation,
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