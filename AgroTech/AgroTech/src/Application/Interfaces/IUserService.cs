using AgroTech.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AgroTech.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDTO>> GetAllAsync();
        Task<UserDTO?> GetByIdAsync(Guid id);
        Task AddAsync(UserDTO dto);
        Task UpdateAsync(UserDTO dto);
        Task DeleteAsync(Guid id);
    }
}