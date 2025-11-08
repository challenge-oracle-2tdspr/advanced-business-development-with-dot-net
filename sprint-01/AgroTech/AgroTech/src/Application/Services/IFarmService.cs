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
    public class FarmService : IFarmService
    {
        private readonly IRepository<Farm> repository;

        public FarmService(IRepository<Farm> repository)
        {
            this.repository = repository;
        }

        public async Task AddAsync(FarmDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new DomainException("O nome da fazenda não pode ser vazio.");

            var farm = new Farm
            {
                Id = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid(),
                Name = dto.Name,
                Location = dto.Location
            };

            await repository.AddAsync(farm);
        }

        public async Task DeleteAsync(Guid id)
        {
            var farm = await repository.GetByIdAsync(id);
            if (farm == null)
                throw new DomainException("Fazenda não encontrada.");

            await repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<FarmDTO>> GetAllAsync()
        {
            var farms = await repository.GetAllAsync();
            return farms.Select(f => new FarmDTO
            {
                Id = f.Id,
                Name = f.Name,
                Location = f.Location
            });
        }

        public async Task<FarmDTO?> GetByIdAsync(Guid id)
        {
            var farm = await repository.GetByIdAsync(id);
            if (farm == null)
                return null;

            return new FarmDTO
            {
                Id = farm.Id,
                Name = farm.Name,
                Location = farm.Location
            };
        }

        public async Task UpdateAsync(FarmDTO dto)
        {
            var farm = await repository.GetByIdAsync(dto.Id);
            if (farm == null)
                throw new DomainException("Fazenda não encontrada.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new DomainException("O nome da fazenda não pode ser vazio.");

            farm.Name = dto.Name;
            farm.Location = dto.Location;

            await repository.UpdateAsync(farm);
        }
    }
}
