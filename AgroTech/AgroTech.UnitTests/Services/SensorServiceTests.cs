using AgroTech.Application.DTOs;
using AgroTech.Application.Exceptions;
using AgroTech.Application.Services;
using AgroTech.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace AgroTech.UnitTests.Services
{
    public class SensorServiceTests
    {
        private readonly Mock<ISensorRepository> _sensorRepositoryMock;
        private readonly SensorService _sensorService;

        public SensorServiceTests()
        {
            _sensorRepositoryMock = new Mock<ISensorRepository>();
            _sensorService = new SensorService(_sensorRepositoryMock.Object);
        }

        [Fact]
        public async Task AddAsync_ListaVazia_DeveLancarDomainException()
        {
            // Arrange
            var sensores = new List<SensorDTO>();

            // Act
            Func<Task> act = async () => await _sensorService.AddAsync(sensores);

            // Assert
            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("A lista de sensores não pode ser vazia.");
        }
    }
}