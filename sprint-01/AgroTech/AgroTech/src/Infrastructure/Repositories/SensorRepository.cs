using AgroTech.Domain.Entities;
using AgroTech.Domain.Interfaces;

namespace AgroTech.Infrastructure.Repositories
{
    public class SensorRepository : InMemoryRepository<Sensor>, ISensorRepository
    {
        public SensorRepository()
        {
            _items.Add(new Sensor { Name = "Sensor Temp", Type = Domain.Enums.SensorType.Temperature, Value = 36 });
            _items.Add(new Sensor { Name = "Sensor Umidade", Type = Domain.Enums.SensorType.Humidity, Value = 25 });
            _items.Add(new Sensor { Name = "Sensor Ph", Type = Domain.Enums.SensorType.Ph, Value = 4.8 });
            _items.Add(new Sensor { Name = "Sensor Luminosidade", Type = Domain.Enums.SensorType.Light, Value = 280 });
            _items.Add(new Sensor { Name = "Sensor Vento", Type = Domain.Enums.SensorType.Wind, Value = 22 });
            _items.Add(new Sensor { Name = "Sensor Temp 2", Type = Domain.Enums.SensorType.Temperature, Value = 38 });
            _items.Add(new Sensor { Name = "Sensor Umidade 2", Type = Domain.Enums.SensorType.Humidity, Value = 20 });
            _items.Add(new Sensor { Name = "Sensor Ph 2", Type = Domain.Enums.SensorType.Ph, Value = 9 });
            _items.Add(new Sensor { Name = "Sensor Light 2", Type = Domain.Enums.SensorType.Light, Value = 100 });
            _items.Add(new Sensor { Name = "Sensor Wind 2", Type = Domain.Enums.SensorType.Wind, Value = 30 });

        }

        public Task<IEnumerable<Sensor>> GetByFarmIdAsync(Guid farmId)
        {
            var sensors = _items.Where(s => s.FarmId == farmId);
            return Task.FromResult(sensors);
        }
    }
}