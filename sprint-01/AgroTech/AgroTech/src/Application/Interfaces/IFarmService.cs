using AgroTech.Application.DTOs;

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

    public interface ICropService
    {
        Task<IEnumerable<CropDTO>> GetAllAsync();
        Task<CropDTO?> GetByIdAsync(Guid id);
        Task AddAsync(CropDTO dto);
        Task UpdateAsync(CropDTO dto);
        Task DeleteAsync(Guid id);
    }

    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetAllAsync();
        Task<UserDTO?> GetByIdAsync(Guid id);
        Task AddAsync(UserDTO dto);
        Task UpdateAsync(UserDTO dto);
        Task DeleteAsync(Guid id);
    }
}