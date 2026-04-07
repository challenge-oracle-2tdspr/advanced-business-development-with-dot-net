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

namespace AgroTech.Tests.CropServiceTests
{
    public class CropServiceTests
    {
        private readonly Mock<IRepository<Crop>> _repositoryMock;
        private readonly CropService _cropService;

        public CropServiceTests()
        {
            _repositoryMock = new Mock<IRepository<Crop>>();
            _cropService = new CropService(_repositoryMock.Object);
        }

        [Fact]
        public async Task AddAsync_ShouldThrowException_WhenNameIsEmpty()
        {
            var cropDto = new CropDTO { Name = "" };

            await Assert.ThrowsAsync<DomainException>(() => _cropService.AddAsync(cropDto));
        }

        [Fact]
        public async Task AddAsync_ShouldCallRepositoryAdd_WhenNameIsValid()
        {
            var cropDto = new CropDTO
            {
                Name = "Crop 1",
                PlantingDate = DateTime.UtcNow,
                FarmId = Guid.NewGuid()
            };

            await _cropService.AddAsync(cropDto);

            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Crop>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllCrops()
        {
            var crops = new List<Crop>
            {
                new Crop { Id = Guid.NewGuid(), Name = "Crop 1", PlantingDate = DateTime.UtcNow, FarmId = Guid.NewGuid() },
                new Crop { Id = Guid.NewGuid(), Name = "Crop 2", PlantingDate = DateTime.UtcNow, FarmId = Guid.NewGuid() }
            };

            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(crops);

            var result = await _cropService.GetAllAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCrop_WhenExists()
        {
            var crop = new Crop { Id = Guid.NewGuid(), Name = "Crop 1", PlantingDate = DateTime.UtcNow, FarmId = Guid.NewGuid() };

            _repositoryMock.Setup(r => r.GetByIdAsync(crop.Id)).ReturnsAsync(crop);

            var result = await _cropService.GetByIdAsync(crop.Id);

            Assert.NotNull(result);
            Assert.Equal(crop.Name, result?.Name);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenCropNotFound()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Crop?)null);

            var cropDto = new CropDTO { Id = Guid.NewGuid(), Name = "New Name", PlantingDate = DateTime.UtcNow, FarmId = Guid.NewGuid() };

            await Assert.ThrowsAsync<DomainException>(() => _cropService.UpdateAsync(cropDto));
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateCrop_WhenValid()
        {
            var crop = new Crop { Id = Guid.NewGuid(), Name = "Old Name", PlantingDate = DateTime.UtcNow, FarmId = Guid.NewGuid() };
            _repositoryMock.Setup(r => r.GetByIdAsync(crop.Id)).ReturnsAsync(crop);

            var cropDto = new CropDTO { Id = crop.Id, Name = "New Name", PlantingDate = DateTime.UtcNow, FarmId = Guid.NewGuid() };

            await _cropService.UpdateAsync(cropDto);

            _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Crop>(c => c.Name == "New Name")), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowException_WhenCropNotFound()
        {
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Crop?)null);

            await Assert.ThrowsAsync<DomainException>(() => _cropService.DeleteAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallDelete_WhenCropExists()
        {
            var crop = new Crop { Id = Guid.NewGuid(), Name = "Crop" };
            _repositoryMock.Setup(r => r.GetByIdAsync(crop.Id)).ReturnsAsync(crop);

            await _cropService.DeleteAsync(crop.Id);

            _repositoryMock.Verify(r => r.DeleteAsync(crop.Id), Times.Once);
        }
    }
}
