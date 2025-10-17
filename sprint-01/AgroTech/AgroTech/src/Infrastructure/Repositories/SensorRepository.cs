using AgroTech.Domain.Entities;
using AgroTech.Domain.Interfaces;

namespace AgroTech.Infrastructure.Repositories
{
    public class SensorRepository : InMemoryRepository<Sensor>, ISensorRepository
    {
        public SensorRepository()
        {
            _items.Add(new Sensor { Name = "Sensor Temp", Type = Domain.Enums.SensorType.Temperature, Value = 25.3 });
            _items.Add(new Sensor { Name = "Sensor Umidade", Type = Domain.Enums.SensorType.Humidity, Value = 78.5 });
            _items.Add(new Sensor { Name = "Sensor PH", Type = Domain.Enums.SensorType.Ph, Value = 6.8 });
        }

        public Task<IEnumerable<Sensor>> GetByFarmIdAsync(Guid farmId)
        {
            var sensors = _items.Where(s => s.FarmId == farmId);
            return Task.FromResult(sensors);
        }
    }
}