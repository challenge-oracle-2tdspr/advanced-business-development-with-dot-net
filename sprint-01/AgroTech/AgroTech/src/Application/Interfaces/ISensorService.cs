using AgroTech.Application.DTOs;

namespace AgroTech.Application.Interfaces
{
    public interface ISensorService
    {
        Task<IEnumerable<SensorDTO>> GetAllAsync();
        Task<SensorDTO?> GetByIdAsync(Guid id);
        Task AddAsync(SensorDTO dto);
    }
}