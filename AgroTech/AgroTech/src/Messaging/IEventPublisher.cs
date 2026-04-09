using AgroTech.Contracts.Events;

namespace AgroTech.Messaging
{
    public interface IEventPublisher
    {
        Task PublishSensorReadingCreatedAsync(
            SensorReadingCreatedEvent @event,
            CancellationToken cancellationToken = default);
    }
}