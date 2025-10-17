using AgroTech.Domain.Entities;
using AgroTech.Domain.Interfaces;

namespace AgroTech.Infrastructure.Repositories
{
    public class InMemorySensorRepository : ISensorRepository
    {
        private readonly List<Sensor> _sensors = new();

        public Task AddAsync(Sensor entity)
        {
            _sensors.Add(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            var sensor = _sensors.FirstOrDefault(s => s.Id == id);
            if (sensor != null) _sensors.Remove(sensor);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Sensor>> GetAllAsync()
        {
            return Task.FromResult(_sensors.AsEnumerable());
        }

        public Task<Sensor?> GetByIdAsync(Guid id)
        {
            var sensor = _sensors.FirstOrDefault(s => s.Id == id);
            return Task.FromResult(sensor);
        }

        public Task<IEnumerable<Sensor>> GetByFarmIdAsync(Guid farmId)
        {
            var sensors = _sensors.Where(s => s.FarmId == farmId);
            return Task.FromResult(sensors);
        }

        public Task UpdateAsync(Sensor entity)
        {
            var sensor = _sensors.FirstOrDefault(s => s.Id == entity.Id);
            if (sensor != null)
            {
                sensor.Name = entity.Name;
                sensor.Type = entity.Type;
                sensor.Value = entity.Value;
                sensor.UpdatedAt = DateTime.UtcNow;
            }
            return Task.CompletedTask;
        }
    }
}