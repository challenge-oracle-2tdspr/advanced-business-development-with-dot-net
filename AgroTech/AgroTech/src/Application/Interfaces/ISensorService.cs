using AgroTech.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgroTech.Application.Interfaces
{
    public interface ISensorService
    {
        Task<IEnumerable<SensorDTO>> GetAllAsync();
        Task<SensorDTO?> GetByIdAsync(Guid id);
        Task AddAsync(SensorDTO dto);
    }
}