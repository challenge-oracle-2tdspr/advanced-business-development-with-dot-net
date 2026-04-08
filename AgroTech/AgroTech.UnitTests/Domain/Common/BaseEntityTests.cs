using AgroTech.Domain.Common;
using FluentAssertions;

namespace AgroTech.UnitTests.Domain.Common
{
    public class BaseEntityTests
    {
        private sealed class FakeEntity : BaseEntity
        {
        }

        [Fact]
        public void BaseEntity_QuandoInstanciada_DeveGerarIdValido()
        {
            // Arrange

            // Act
            var entity = new FakeEntity();

            // Assert
            entity.Id.Should().NotBeEmpty();
        }

        [Fact]
        public void BaseEntity_QuandoInstanciada_DevePreencherCreatedAt()
        {
            // Arrange
            var before = DateTime.UtcNow.AddSeconds(-5);

            // Act
            var entity = new FakeEntity();

            // Assert
            var after = DateTime.UtcNow.AddSeconds(5);

            entity.CreatedAt.Should().BeOnOrAfter(before);
            entity.CreatedAt.Should().BeOnOrBefore(after);
        }

        [Fact]
        public void BaseEntity_QuandoInstanciada_DeveTerUpdatedAtNulo()
        {
            // Arrange

            // Act
            var entity = new FakeEntity();

            // Assert
            entity.UpdatedAt.Should().BeNull();
        }
    }
}