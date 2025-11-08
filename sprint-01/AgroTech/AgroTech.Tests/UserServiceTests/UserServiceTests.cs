using AgroTech.Application.DTOs;
using AgroTech.Application.Exceptions;
using AgroTech.Application.Services;
using AgroTech.Domain.Entities;
using AgroTech.Domain.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AgroTech.Tests.UserServiceTests
{
    public class UserServiceTests
    {
        private readonly Mock<IRepository<User>> _repositoryMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _repositoryMock = new Mock<IRepository<User>>();
            _userService = new UserService(_repositoryMock.Object);
        }

        [Fact]
        public async Task AddAsync_ShouldThrowException_WhenNameIsEmpty()
        {
            var userDto = new UserDTO { Name = "" };

            await Assert.ThrowsAsync<DomainException>(() => _userService.AddAsync(userDto));
        }

        [Fact]
        public async Task AddAsync_ShouldThrowException_WhenEmailIsEmpty()
        {
            var userDto = new UserDTO { Name = "User", Email = "" };

            await Assert.ThrowsAsync<DomainException>(() => _userService.AddAsync(userDto));
        }

        [Fact]
        public async Task AddAsync_ShouldCallRepositoryAdd_WhenValid()
        {
            var userDto = new UserDTO { Name = "User", Email = "user@example.com", Role = "Producer" };

            await _userService.AddAsync(userDto);

            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllUsers()
        {
            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), Name = "User 1", Email = "user1@example.com", Role = "Producer" },
                new User { Id = Guid.NewGuid(), Name = "User 2", Email = "user2@example.com", Role = "Admin" }
            };

            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

            var result = await _userService.GetAllAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnUser_WhenExists()
        {
            var user = new User { Id = Guid.NewGuid(), Name = "User 1", Email = "user1@example.com", Role = "Producer" };

            _repositoryMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

            var result = await _userService.GetByIdAsync(user.Id);

            Assert.NotNull(result);
            Assert.Equal(user.Name, result?.Name);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenUserNotFound()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

            var userDto = new UserDTO { Id = Guid.NewGuid(), Name = "New Name", Email = "new@example.com", Role = "User" };

            await Assert.ThrowsAsync<DomainException>(() => _userService.UpdateAsync(userDto));
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenNameIsEmpty()
        {
            var user = new User { Id = Guid.NewGuid(), Name = "Old Name", Email = "old@example.com", Role = "User" };
            _repositoryMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

            var userDto = new UserDTO { Id = user.Id, Name = "", Email = "new@example.com", Role = "User" };

            await Assert.ThrowsAsync<DomainException>(() => _userService.UpdateAsync(userDto));
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenEmailIsEmpty()
        {
            var user = new User { Id = Guid.NewGuid(), Name = "Old Name", Email = "old@example.com", Role = "User" };
            _repositoryMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

            var userDto = new UserDTO { Id = user.Id, Name = "New Name", Email = "", Role = "User" };

            await Assert.ThrowsAsync<DomainException>(() => _userService.UpdateAsync(userDto));
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateUser_WhenValid()
        {
            var user = new User { Id = Guid.NewGuid(), Name = "Old Name", Email = "old@example.com", Role = "User" };
            _repositoryMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

            var userDto = new UserDTO { Id = user.Id, Name = "New Name", Email = "new@example.com", Role = "Admin" };

            await _userService.UpdateAsync(userDto);

            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<User>(u => u.Name == "New Name" && u.Email == "new@example.com" && u.Role == "Admin")), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowException_WhenUserNotFound()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<DomainException>(() => _userService.DeleteAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallDelete_WhenUserExists()
        {
            var user = new User { Id = Guid.NewGuid(), Name = "User" };
            _repositoryMock.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);

            await _userService.DeleteAsync(user.Id);

            _repositoryMock.Verify(r => r.DeleteAsync(user.Id), Times.Once);
        }
    }
}
