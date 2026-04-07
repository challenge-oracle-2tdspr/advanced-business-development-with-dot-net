using AgroTech.Application.DTOs;
using AgroTech.Application.Exceptions;
using AgroTech.Application.Interfaces;
using AgroTech.Domain.Entities;
using AgroTech.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new DomainException("O nome do sensor não pode ser vazio.");

            if (string.IsNullOrWhiteSpace(dto.Type))
                throw new DomainException("O tipo do sensor não pode ser vazio.");

            if (!int.TryParse(dto.Type, out var sensorType))
                throw new DomainException("O tipo do sensor deve ser numérico.");

            var sensor = new Sensor
            {
                Id = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid(),
                Name = dto.Name,
                Type = sensorType,
                Value = dto.Value,
                Timestamp = dto.Timestamp,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(sensor);
        }

        public async Task DeleteAsync(Guid id)
        {
            var sensor = await _repository.GetByIdAsync(id);
            if (sensor == null)
                throw new DomainException("Sensor não encontrado.");

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
                Timestamp = s.Timestamp
            });
        }

        public async Task<SensorDTO?> GetByIdAsync(Guid id)
        {
            var sensor = await _repository.GetByIdAsync(id);
            if (sensor == null)
                return null;

            return new SensorDTO
            {
                Id = sensor.Id,
                Name = sensor.Name,
                Type = sensor.Type.ToString(),
                Value = sensor.Value,
                Timestamp = sensor.Timestamp
            };
        }

        public async Task UpdateAsync(SensorDTO dto)
        {
            var sensor = await _repository.GetByIdAsync(dto.Id);
            if (sensor == null)
                throw new DomainException("Sensor não encontrado.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new DomainException("O nome do sensor não pode ser vazio.");

            if (string.IsNullOrWhiteSpace(dto.Type))
                throw new DomainException("O tipo do sensor não pode ser vazio.");

            if (!int.TryParse(dto.Type, out var sensorType))
                throw new DomainException("O tipo do sensor deve ser numérico.");

            sensor.Name = dto.Name;
            sensor.Type = sensorType;
            sensor.Value = dto.Value;
            sensor.Timestamp = dto.Timestamp;
            sensor.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(sensor);
        }
    }
}