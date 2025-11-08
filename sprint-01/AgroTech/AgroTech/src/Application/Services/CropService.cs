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
        private readonly IRepository<Crop> repository;

        public CropService(IRepository<Crop> repository)
        {
            this.repository = repository;
        }

        public async Task AddAsync(CropDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new DomainException("O nome da safra não pode ser vazio.");
            if (dto.PlantingDate == default)
                throw new DomainException("Data de plantio inválida.");
            if (dto.FarmId == Guid.Empty)
                throw new DomainException("Fazenda associada obrigatória.");

            var crop = new Crop
            {
                Id = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid(),
                Name = dto.Name,
                PlantingDate = dto.PlantingDate,
                HarvestDate = dto.HarvestDate,
                FarmId = dto.FarmId
            };

            await repository.AddAsync(crop);
        }

        public async Task DeleteAsync(Guid id)
        {
            var crop = await repository.GetByIdAsync(id);
            if (crop == null)
                throw new DomainException("Safra não encontrada.");

            await repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<CropDTO>> GetAllAsync()
        {
            var crops = await repository.GetAllAsync();
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
            var crop = await repository.GetByIdAsync(id);
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
            var crop = await repository.GetByIdAsync(dto.Id);
            if (crop == null)
                throw new DomainException("Safra não encontrada.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new DomainException("O nome da safra não pode ser vazio.");
            if (dto.PlantingDate == default)
                throw new DomainException("Data de plantio inválida.");

            crop.Name = dto.Name;
            crop.PlantingDate = dto.PlantingDate;
            crop.HarvestDate = dto.HarvestDate;
            crop.FarmId = dto.FarmId;

            await repository.UpdateAsync(crop);
        }
    }
}
