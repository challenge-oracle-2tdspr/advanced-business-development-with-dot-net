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
        Task<List<Guid>> AddAsync(IEnumerable<SensorDTO> dtos);
        Task UpdateAsync(SensorDTO dto);
        Task DeleteAsync(Guid id);
        Task<PagedResultDTO<SensorDTO>> SearchAsync(SensorSearchDTO searchDto);
    }
}