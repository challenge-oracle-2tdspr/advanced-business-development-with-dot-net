using AgroTech.Application.DTOs;
using AgroTech.Application.Interfaces;
using AgroTech.Domain.Entities;
using AgroTech.Domain.Interfaces;

namespace AgroTech.Application.Services
{
    public class SensorService : ISensorService
    {
        private readonly ISensorRepository _repository;

        public SensorService(ISensorRepository repository)
        {
            _repository = repository;
        }

        public async Task AddAsync(SensorDTO dto)
        {
            var sensor = new Sensor
            {
                Id = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid(),
                Name = dto.Name,
                Type = Enum.Parse<Domain.Enums.SensorType>(dto.Type),
                Value = dto.Value,
                UpdatedAt = dto.Timestamp
            };
            await _repository.AddAsync(sensor);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<SensorDTO>> GetAllAsync()
        {
            var sensors = await _repository.GetAllAsync();
            return sensors.Select(s => new SensorDTO
            {
                Id = s.Id,
                Name = s.Name,
                Type = s.Type.ToString(),
                Value = s.Value,
                Timestamp = s.UpdatedAt ?? DateTime.UtcNow
            });
        }

        public async Task<SensorDTO?> GetByIdAsync(Guid id)
        {
            var s = await _repository.GetByIdAsync(id);
            if (s == null) return null;

            return new SensorDTO
            {
                Id = s.Id,
                Name = s.Name,
                Type = s.Type.ToString(),
                Value = s.Value,
                Timestamp = s.UpdatedAt ?? DateTime.UtcNow
            };
        }

        public async Task UpdateAsync(SensorDTO dto)
        {
            var sensor = await _repository.GetByIdAsync(dto.Id);
            if (sensor == null) throw new Exception("Sensor não encontrado");

            sensor.Name = dto.Name;
            sensor.Type = Enum.Parse<Domain.Enums.SensorType>(dto.Type);
            sensor.Value = dto.Value;
            sensor.UpdatedAt = dto.Timestamp;

            await _repository.UpdateAsync(sensor);
        }
    }
}
