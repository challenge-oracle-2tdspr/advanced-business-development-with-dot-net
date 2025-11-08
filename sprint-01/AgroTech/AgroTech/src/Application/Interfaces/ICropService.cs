using AgroTech.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgroTech.Application.Interfaces
{
    public interface ICropService
    {
        Task<IEnumerable<CropDTO>> GetAllAsync();
        Task<CropDTO?> GetByIdAsync(Guid id);
        Task AddAsync(CropDTO dto);
        Task UpdateAsync(CropDTO dto);
        Task DeleteAsync(Guid id);
    }
}