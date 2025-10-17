using AgroTech.Application.DTOs;
using AgroTech.Application.Interfaces;
using AgroTech.Domain.Entities;
using AgroTech.Domain.Interfaces;

namespace AgroTech.Application.Services
{
    public class SensorService : ISensorService
    {
        private readonly ISensorRepository _sensorRepository;

        public SensorService(ISensorRepository sensorRepository)
        {
            _sensorRepository = sensorRepository;
        }

        public async Task<IEnumerable<SensorDTO>> GetAllAsync()
        {
            var sensors = await _sensorRepository.GetAllAsync();
            return sensors.Select(s => new SensorDTO
            {
                Id = s.Id,
                Name = s.Name,
                Type = s.Type.ToString(),
                Value = s.Value,
                Timestamp = s.UpdatedAt ?? s.CreatedAt
            });
        }

        public async Task<SensorDTO?> GetByIdAsync(Guid id)
        {
            var s = await _sensorRepository.GetByIdAsync(id);
            if (s == null) return null;

            return new SensorDTO
            {
                Id = s.Id,
                Name = s.Name,
                Type = s.Type.ToString(),
                Value = s.Value,
                Timestamp = s.UpdatedAt ?? s.CreatedAt
            };
        }

        public async Task AddAsync(SensorDTO dto)
        {
            if (dto.Value < 0)
                throw new ArgumentException("Valor do sensor não pode ser negativo.");

            var sensor = new Sensor
            {
                Name = dto.Name,
                Type = Enum.Parse<Domain.Enums.SensorType>(dto.Type),
                Value = dto.Value
            };

            await _sensorRepository.AddAsync(sensor);
        }
    }
}