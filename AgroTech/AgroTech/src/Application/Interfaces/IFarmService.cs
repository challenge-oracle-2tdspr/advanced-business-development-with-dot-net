using AgroTech.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgroTech.Application.Interfaces
{
    public interface IFarmService
    {
        Task<IEnumerable<FarmDTO>> GetAllAsync();
        Task<FarmDTO?> GetByIdAsync(Guid id);
        Task AddAsync(FarmDTO dto);
        Task UpdateAsync(FarmDTO dto);
        Task DeleteAsync(Guid id);
    }
}