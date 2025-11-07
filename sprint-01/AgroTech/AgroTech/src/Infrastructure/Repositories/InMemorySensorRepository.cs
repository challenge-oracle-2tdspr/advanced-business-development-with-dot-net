using AgroTech.Domain.Entities;
using AgroTech.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgroTech.Infrastructure.Repositories
{
    public class InMemorySensorRepository : InMemoryRepository<Sensor>, ISensorRepository
    {
        public InMemorySensorRepository()
        {
            _items.Add(new Sensor { Name = "Sensor Temp", Type = Domain.Enums.SensorType.Temperature, Value = 25.3 });
            _items.Add(new Sensor { Name = "Sensor Umidade", Type = Domain.Enums.SensorType.Humidity, Value = 78.5 });
            _items.Add(new Sensor { Name = "Sensor Ph", Type = Domain.Enums.SensorType.Ph, Value = 6.8 });
        }

        public Task<IEnumerable<Sensor>> GetByFarmIdAsync(Guid farmId)
        {
            var sensors = _items.Where(s => s.FarmId == farmId);
            return Task.FromResult(sensors);
        }
    }
}