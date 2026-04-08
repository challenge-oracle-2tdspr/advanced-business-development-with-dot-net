using AgroTech.Domain.Common;
using AgroTech.Domain.Entities;
using FluentAssertions;

namespace AgroTech.UnitTests.Domain.Entities
{
    public class SensorTests
    {
        [Fact]
        public void Sensor_DeveHerdarDeBaseEntity()
        {
            // Arrange

            // Act
            var sensor = new Sensor();

            // Assert
            sensor.Should().BeAssignableTo<BaseEntity>();
        }

        [Fact]
        public void Sensor_QuandoInstanciado_DevePossuirValoresPadraoEsperados()
        {
            // Arrange

            // Act
            var sensor = new Sensor();

            // Assert
            sensor.Id.Should().NotBeEmpty();
            sensor.CreatedAt.Should().NotBe(default);
            sensor.UpdatedAt.Should().BeNull();
            sensor.Name.Should().BeEmpty();
            sensor.Type.Should().Be(0);
            sensor.Value.Should().Be(0);
            sensor.Timestamp.Should().Be(default);
        }

        [Fact]
        public void Sensor_QuandoPropriedadesForemAtribuidas_DeveManterOsValoresInformados()
        {
            // Arrange
            var timestamp = new DateTime(2026, 4, 8, 12, 0, 0, DateTimeKind.Utc);

            // Act
            var sensor = new Sensor
            {
                Name = "Temperatura",
                Type = 1,
                Value = 25.4,
                Timestamp = timestamp
            };

            // Assert
            sensor.Name.Should().Be("Temperatura");
            sensor.Type.Should().Be(1);
            sensor.Value.Should().Be(25.4);
            sensor.Timestamp.Should().Be(timestamp);
        }

        [Fact]
        public void Sensor_QuandoUpdatedAtForAtribuido_DeveAceitarONovoValor()
        {
            // Arrange
            var updatedAt = new DateTime(2026, 4, 8, 13, 0, 0, DateTimeKind.Utc);

            // Act
            var sensor = new Sensor
            {
                UpdatedAt = updatedAt
            };

            // Assert
            sensor.UpdatedAt.Should().Be(updatedAt);
        }
    }
}