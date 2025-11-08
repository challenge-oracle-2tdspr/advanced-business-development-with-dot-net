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
        private readonly IRepository<User> repository;

        public UserService(IRepository<User> repository)
        {
            this.repository = repository;
        }

        public async Task AddAsync(UserDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new DomainException("Nome de usuário não pode ser vazio.");
            if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains("@"))
                throw new DomainException("Email inválido.");
            if (string.IsNullOrWhiteSpace(dto.Role))
                dto.Role = "Producer";

            var user = new User
            {
                Id = dto.Id != Guid.Empty ? dto.Id : Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email,
                Role = dto.Role,
            };

            await repository.AddAsync(user);
        }


        public async Task DeleteAsync(Guid id)
        {
            var user = await repository.GetByIdAsync(id);
            if (user == null)
                throw new DomainException("Usuário não encontrado.");

            await repository.DeleteAsync(id);
        }

        public async Task<IEnumerable<UserDTO>> GetAllAsync()
        {
            var users = await repository.GetAllAsync();
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
            var user = await repository.GetByIdAsync(id);
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
            var user = await repository.GetByIdAsync(dto.Id);
            if (user == null)
                throw new DomainException("Usuário não encontrado.");

            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new DomainException("Nome de usuário não pode ser vazio.");
            if (string.IsNullOrWhiteSpace(dto.Email) || !dto.Email.Contains("@"))
                throw new DomainException("Email inválido.");

            user.Name = dto.Name;
            user.Email = dto.Email;
            user.Role = string.IsNullOrWhiteSpace(dto.Role) ? "Producer" : dto.Role;

            await repository.UpdateAsync(user);
        }
    }
}
