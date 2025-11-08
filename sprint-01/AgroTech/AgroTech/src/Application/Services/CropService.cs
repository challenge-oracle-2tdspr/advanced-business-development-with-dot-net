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
    public class CropService : ICropService
    {
        private readonly IRepository<Crop> _repository;

        public CropService(IRepository<Crop> repository)
        {
            _repository = repository;
        }

        public async Task AddAsync(CropDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new DomainException("O nome da safra não pode ser vazio.");

            var crop = new Crop
            {
                Id = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid(),
                Name = dto.Name,
                PlantingDate = dto.PlantingDate,
                HarvestDate = dto.HarvestDate,
                FarmId = dto.FarmId,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(crop);
        }

        public async Task DeleteAsync(Guid id)
        {
            var crop = await _repository.GetByIdAsync(id);
            if (crop == null)
                throw new DomainException("Safra não encontrada.");

            await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<CropDTO>> GetAllAsync()
        {
            var crops = await _repository.GetAllAsync();
            return crops.Select(c => new CropDTO
            {
                Id = c.Id,
                Name = c.Name,
                PlantingDate = c.PlantingDate,
                HarvestDate = c.HarvestDate,
                FarmId = c.FarmId
            });
        }

        public async Task<CropDTO?> GetByIdAsync(Guid id)
        {
            var crop = await _repository.GetByIdAsync(id);
            if (crop == null)
                return null;

            return new CropDTO
            {
                Id = crop.Id,
                Name = crop.Name,
                PlantingDate = crop.PlantingDate,
                HarvestDate = crop.HarvestDate,
                FarmId = crop.FarmId
            };
        }

        public async Task UpdateAsync(CropDTO dto)
        {
            var crop = await _repository.GetByIdAsync(dto.Id);
            if (crop == null)
                throw new DomainException("Safra não encontrada.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new DomainException("O nome da safra não pode ser vazio.");

            crop.Name = dto.Name;
            crop.PlantingDate = dto.PlantingDate;
            crop.HarvestDate = dto.HarvestDate;
            crop.FarmId = dto.FarmId;
            crop.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(crop);
        }
    }
}
