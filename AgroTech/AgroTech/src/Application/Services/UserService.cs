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
    public class UserService : IUserService
    {
        private readonly IRepository<User> _repository;

        public UserService(IRepository<User> repository)
        {
            _repository = repository;
        }

        public async Task AddAsync(UserDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new DomainException("O nome do usuário não pode ser vazio.");

            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new DomainException("O e-mail não pode ser vazio.");

            var user = new User
            {
                Id = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email,
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(user);
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
                throw new DomainException("Usuário não encontrado.");

            await _repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<UserDTO>> GetAllAsync()
        {
            var users = await _repository.GetAllAsync();
            return users.Select(u => new UserDTO
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role
            });
        }

        public async Task<UserDTO?> GetByIdAsync(Guid id)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
                return null;

            return new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task UpdateAsync(UserDTO dto)
        {
            var user = await _repository.GetByIdAsync(dto.Id);
            if (user == null)
                throw new DomainException("Usuário não encontrado.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new DomainException("O nome do usuário não pode ser vazio.");

            if (string.IsNullOrWhiteSpace(dto.Email))
                throw new DomainException("O e-mail não pode ser vazio.");

            user.Name = dto.Name;
            user.Email = dto.Email;
            user.Role = dto.Role;
            user.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(user);
        }
    }
}
