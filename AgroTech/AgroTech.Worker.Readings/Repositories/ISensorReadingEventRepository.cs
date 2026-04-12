using AgroTech.Worker.Readings.Models;

namespace AgroTech.Worker.Readings.Repositories
{
    public interface ISensorReadingEventRepository
    {
        Task SaveAsync(SensorReadingEventRecord record, CancellationToken cancellationToken);
    }
}
