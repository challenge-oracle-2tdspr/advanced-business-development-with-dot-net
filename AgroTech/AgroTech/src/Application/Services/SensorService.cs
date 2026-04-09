using AgroTech.Application.DTOs;
using AgroTech.Application.Exceptions;
using AgroTech.Application.Interfaces;
using AgroTech.Domain.Entities;
using AgroTech.Domain.Interfaces;
using AgroTech.Contracts.Events;
using AgroTech.Messaging;

namespace AgroTech.Application.Services
{
    public class SensorService : ISensorService
    {
        private readonly ISensorRepository _repository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICorrelationIdAccessor _correlationIdAccessor;

        public SensorService(
            ISensorRepository repository,
            IEventPublisher eventPublisher,
            ICorrelationIdAccessor correlationIdAccessor)
        {
            _repository = repository;
            _eventPublisher = eventPublisher;
            _correlationIdAccessor = correlationIdAccessor;
        }

        public async Task<List<Guid>> AddAsync(IEnumerable<SensorDTO> dtos)
        {
            if (dtos == null || !dtos.Any())
                throw new DomainException("A lista de sensores não pode ser vazia.");

            var ids = new List<Guid>();
            var correlationId = _correlationIdAccessor.GetCorrelationId();

            foreach (var dto in dtos)
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                    throw new DomainException("O nome do sensor não pode ser vazio.");

                if (string.IsNullOrWhiteSpace(dto.Type))
                    throw new DomainException("O tipo do sensor não pode ser vazio.");

                if (!int.TryParse(dto.Type, out var sensorType))
                    throw new DomainException("O tipo do sensor deve ser numérico.");

                var sensorId = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid();

                var sensor = new Sensor
                {
                    Id = sensorId,
                    Name = dto.Name,
                    Type = sensorType,
                    Value = dto.Value,
                    Timestamp = dto.Timestamp,
                    CreatedAt = DateTime.UtcNow
                };

                await _repository.AddAsync(sensor);

                var sensorEvent = new SensorReadingCreatedEvent
                {
                    CorrelationId = correlationId,
                    SensorId = sensor.Id,
                    SensorName = sensor.Name,
                    SensorType = sensor.Type,
                    Value = Convert.ToDouble(sensor.Value),
                    Timestamp = sensor.Timestamp,
                    Source = "node-red"
                };

                await _eventPublisher.PublishSensorReadingCreatedAsync(sensorEvent);

                ids.Add(sensorId);
            }

            return ids;
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

        public async Task<PagedResultDTO<SensorDTO>> SearchAsync(SensorSearchDTO searchDto)
        {
            var sensors = await _repository.GetAllAsync();

            var query = sensors.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchDto.Name))
                query = query.Where(s => s.Name.ToLower().Contains(searchDto.Name.ToLower()));

            if (!string.IsNullOrWhiteSpace(searchDto.Type) && int.TryParse(searchDto.Type, out var sensorType))
                query = query.Where(s => s.Type == sensorType);

            if (searchDto.MinValue.HasValue)
                query = query.Where(s => s.Value >= searchDto.MinValue.Value);

            if (searchDto.MaxValue.HasValue)
                query = query.Where(s => s.Value <= searchDto.MaxValue.Value);

            if (searchDto.StartTimestamp.HasValue)
                query = query.Where(s => s.Timestamp >= searchDto.StartTimestamp.Value);

            if (searchDto.EndTimestamp.HasValue)
                query = query.Where(s => s.Timestamp <= searchDto.EndTimestamp.Value);

            query = (searchDto.OrderBy?.ToLower(), searchDto.Direction?.ToLower()) switch
            {
                ("name", "asc") => query.OrderBy(s => s.Name),
                ("name", "desc") => query.OrderByDescending(s => s.Name),
                ("type", "asc") => query.OrderBy(s => s.Type),
                ("type", "desc") => query.OrderByDescending(s => s.Type),
                ("value", "asc") => query.OrderBy(s => s.Value),
                ("value", "desc") => query.OrderByDescending(s => s.Value),
                ("timestamp", "asc") => query.OrderBy(s => s.Timestamp),
                _ => query.OrderByDescending(s => s.Timestamp)
            };

            var totalItems = query.Count();

            var items = query
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .Select(s => new SensorDTO
                {
                    Id = s.Id,
                    Name = s.Name,
                    Type = s.Type.ToString(),
                    Value = s.Value,
                    Timestamp = s.Timestamp
                })
                .ToList();

            return new PagedResultDTO<SensorDTO>
            {
                Items = items,
                Page = searchDto.Page,
                PageSize = searchDto.PageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)searchDto.PageSize)
            };
        }
    }
}