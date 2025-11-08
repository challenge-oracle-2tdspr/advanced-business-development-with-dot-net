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

namespace AgroTech.Tests.FarmServiceTests
{
    public class FarmServiceTests
    {
        private readonly Mock<IRepository<Farm>> _repositoryMock;
        private readonly FarmService _farmService;

        public FarmServiceTests()
        {
            _repositoryMock = new Mock<IRepository<Farm>>();
            _farmService = new FarmService(_repositoryMock.Object);
        }

        [Fact]
        public async Task AddAsync_ShouldThrowException_WhenNameIsEmpty()
        {
            var farmDto = new FarmDTO { Name = "" };

            await Assert.ThrowsAsync<DomainException>(() => _farmService.AddAsync(farmDto));
        }

        [Fact]
        public async Task AddAsync_ShouldCallRepositoryAdd_WhenNameIsValid()
        {
            var farmDto = new FarmDTO { Name = "Farm 1", Location = "Loc 1" };

            await _farmService.AddAsync(farmDto);

            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Farm>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllFarms()
        {
            var farms = new List<Farm>
            {
                new Farm { Id = Guid.NewGuid(), Name = "Farm 1", Location = "Loc 1" },
                new Farm { Id = Guid.NewGuid(), Name = "Farm 2", Location = "Loc 2" }
            };

            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(farms);

            var result = await _farmService.GetAllAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnFarm_WhenExists()
        {
            var farm = new Farm { Id = Guid.NewGuid(), Name = "Farm 1", Location = "Loc 1" };

            _repositoryMock.Setup(r => r.GetByIdAsync(farm.Id)).ReturnsAsync(farm);

            var result = await _farmService.GetByIdAsync(farm.Id);

            Assert.NotNull(result);
            Assert.Equal(farm.Name, result?.Name);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenFarmNotFound()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Farm?)null);

            var farmDto = new FarmDTO { Id = Guid.NewGuid(), Name = "New Name", Location = "New Location" };

            await Assert.ThrowsAsync<DomainException>(() => _farmService.UpdateAsync(farmDto));
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateFarm_WhenValid()
        {
            var farm = new Farm { Id = Guid.NewGuid(), Name = "Old Name", Location = "Old Location" };
            _repositoryMock.Setup(r => r.GetByIdAsync(farm.Id)).ReturnsAsync(farm);

            var farmDto = new FarmDTO { Id = farm.Id, Name = "New Name", Location = "New Location" };

            await _farmService.UpdateAsync(farmDto);

            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Farm>(f => f.Name == "New Name" && f.Location == "New Location")), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowException_WhenFarmNotFound()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Farm?)null);

            await Assert.ThrowsAsync<DomainException>(() => _farmService.DeleteAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallDelete_WhenFarmExists()
        {
            var farm = new Farm { Id = Guid.NewGuid(), Name = "Farm" };
            _repositoryMock.Setup(r => r.GetByIdAsync(farm.Id)).ReturnsAsync(farm);

            await _farmService.DeleteAsync(farm.Id);

            _repositoryMock.Verify(r => r.DeleteAsync(farm.Id), Times.Once);
        }
    }
}
