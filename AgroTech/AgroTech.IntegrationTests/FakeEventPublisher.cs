using AgroTech.Contracts.Events;
using AgroTech.Messaging;

namespace AgroTech.IntegrationTests
{
    public class FakeEventPublisher : IEventPublisher
    {
        public Task PublishSensorReadingCreatedAsync(
            SensorReadingCreatedEvent sensorEvent,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}