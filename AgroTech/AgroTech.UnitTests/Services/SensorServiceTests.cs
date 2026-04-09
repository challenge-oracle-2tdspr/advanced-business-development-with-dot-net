using AgroTech.Application.DTOs;
using AgroTech.Application.Exceptions;
using AgroTech.Application.Services;
using AgroTech.Domain.Entities;
using AgroTech.Domain.Interfaces;
using AgroTech.Contracts.Events;
using AgroTech.Messaging;
using FluentAssertions;
using Moq;

namespace AgroTech.UnitTests.Services
{
    public class SensorServiceTests
    {
        private readonly Mock<ISensorRepository> _sensorRepositoryMock;
        private readonly Mock<IEventPublisher> _eventPublisherMock;
        private readonly Mock<ICorrelationIdAccessor> _correlationIdAccessorMock;
        private readonly SensorService _sensorService;

        public SensorServiceTests()
        {
            _sensorRepositoryMock = new Mock<ISensorRepository>();
            _eventPublisherMock = new Mock<IEventPublisher>();
            _correlationIdAccessorMock = new Mock<ICorrelationIdAccessor>();

            _correlationIdAccessorMock
                .Setup(x => x.GetCorrelationId())
                .Returns("test-correlation-id");

            _eventPublisherMock
                .Setup(x => x.PublishSensorReadingCreatedAsync(
                    It.IsAny<SensorReadingCreatedEvent>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _sensorService = new SensorService(
                _sensorRepositoryMock.Object,
                _eventPublisherMock.Object,
                _correlationIdAccessorMock.Object);
        }

        [Fact]
        public async Task AddAsync_ListaNula_DeveLancarDomainException()
        {
            IEnumerable<SensorDTO>? sensores = null;

            Func<Task> act = async () => await _sensorService.AddAsync(sensores!);

            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("A lista de sensores não pode ser vazia.");

            _sensorRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Sensor>()), Times.Never);
            _eventPublisherMock.Verify(
                x => x.PublishSensorReadingCreatedAsync(It.IsAny<SensorReadingCreatedEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task AddAsync_ListaVazia_DeveLancarDomainException()
        {
            var sensores = new List<SensorDTO>();

            Func<Task> act = async () => await _sensorService.AddAsync(sensores);

            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("A lista de sensores não pode ser vazia.");

            _sensorRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Sensor>()), Times.Never);
            _eventPublisherMock.Verify(
                x => x.PublishSensorReadingCreatedAsync(It.IsAny<SensorReadingCreatedEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task AddAsync_NomeVazio_DeveLancarDomainException()
        {
            var sensores = new List<SensorDTO>
            {
                new()
                {
                    Name = "",
                    Type = "1",
                    Value = 25.4,
                    Timestamp = DateTime.UtcNow
                }
            };

            Func<Task> act = async () => await _sensorService.AddAsync(sensores);

            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("O nome do sensor não pode ser vazio.");

            _sensorRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Sensor>()), Times.Never);
            _eventPublisherMock.Verify(
                x => x.PublishSensorReadingCreatedAsync(It.IsAny<SensorReadingCreatedEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task AddAsync_TipoVazio_DeveLancarDomainException()
        {
            var sensores = new List<SensorDTO>
            {
                new()
                {
                    Name = "Temperatura",
                    Type = "",
                    Value = 25.4,
                    Timestamp = DateTime.UtcNow
                }
            };

            Func<Task> act = async () => await _sensorService.AddAsync(sensores);

            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("O tipo do sensor não pode ser vazio.");

            _sensorRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Sensor>()), Times.Never);
            _eventPublisherMock.Verify(
                x => x.PublishSensorReadingCreatedAsync(It.IsAny<SensorReadingCreatedEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task AddAsync_TipoNaoNumerico_DeveLancarDomainException()
        {
            var sensores = new List<SensorDTO>
            {
                new()
                {
                    Name = "Temperatura",
                    Type = "abc",
                    Value = 25.4,
                    Timestamp = DateTime.UtcNow
                }
            };

            Func<Task> act = async () => await _sensorService.AddAsync(sensores);

            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("O tipo do sensor deve ser numérico.");

            _sensorRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Sensor>()), Times.Never);
            _eventPublisherMock.Verify(
                x => x.PublishSensorReadingCreatedAsync(It.IsAny<SensorReadingCreatedEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task AddAsync_DadosValidos_DeveSalvarSensoresERetornarIds()
        {
            var timestamp1 = new DateTime(2026, 4, 7, 10, 0, 0, DateTimeKind.Utc);
            var timestamp2 = new DateTime(2026, 4, 7, 10, 5, 0, DateTimeKind.Utc);

            var sensores = new List<SensorDTO>
            {
                new()
                {
                    Name = "Temperatura",
                    Type = "1",
                    Value = 25.4,
                    Timestamp = timestamp1
                },
                new()
                {
                    Name = "Umidade",
                    Type = "2",
                    Value = 60,
                    Timestamp = timestamp2
                }
            };

            var sensoresSalvos = new List<Sensor>();
            var eventosPublicados = new List<SensorReadingCreatedEvent>();

            _sensorRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Sensor>()))
                .Callback<Sensor>(sensor => sensoresSalvos.Add(sensor))
                .Returns(Task.CompletedTask);

            _eventPublisherMock
                .Setup(x => x.PublishSensorReadingCreatedAsync(
                    It.IsAny<SensorReadingCreatedEvent>(),
                    It.IsAny<CancellationToken>()))
                .Callback<SensorReadingCreatedEvent, CancellationToken>((evt, _) => eventosPublicados.Add(evt))
                .Returns(Task.CompletedTask);

            var result = await _sensorService.AddAsync(sensores);

            result.Should().HaveCount(2);
            result.Should().OnlyHaveUniqueItems();

            _sensorRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Sensor>()), Times.Exactly(2));
            _eventPublisherMock.Verify(
                x => x.PublishSensorReadingCreatedAsync(It.IsAny<SensorReadingCreatedEvent>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));

            sensoresSalvos.Should().HaveCount(2);

            sensoresSalvos[0].Name.Should().Be("Temperatura");
            sensoresSalvos[0].Type.Should().Be(1);
            sensoresSalvos[0].Value.Should().Be(25.4);
            sensoresSalvos[0].Timestamp.Should().Be(timestamp1);
            sensoresSalvos[0].Id.Should().NotBeEmpty();

            sensoresSalvos[1].Name.Should().Be("Umidade");
            sensoresSalvos[1].Type.Should().Be(2);
            sensoresSalvos[1].Value.Should().Be(60);
            sensoresSalvos[1].Timestamp.Should().Be(timestamp2);
            sensoresSalvos[1].Id.Should().NotBeEmpty();

            result.Should().Contain(sensoresSalvos[0].Id);
            result.Should().Contain(sensoresSalvos[1].Id);

            eventosPublicados.Should().HaveCount(2);

            eventosPublicados[0].EventName.Should().Be("sensor.reading.created");
            eventosPublicados[0].CorrelationId.Should().Be("test-correlation-id");
            eventosPublicados[0].SensorName.Should().Be("Temperatura");
            eventosPublicados[0].SensorType.Should().Be(1);
            eventosPublicados[0].Value.Should().Be(25.4);
            eventosPublicados[0].Timestamp.Should().Be(timestamp1);
            eventosPublicados[0].Source.Should().Be("node-red");

            eventosPublicados[1].EventName.Should().Be("sensor.reading.created");
            eventosPublicados[1].CorrelationId.Should().Be("test-correlation-id");
            eventosPublicados[1].SensorName.Should().Be("Umidade");
            eventosPublicados[1].SensorType.Should().Be(2);
            eventosPublicados[1].Value.Should().Be(60);
            eventosPublicados[1].Timestamp.Should().Be(timestamp2);
            eventosPublicados[1].Source.Should().Be("node-red");
        }

        [Fact]
        public async Task UpdateAsync_SensorNaoEncontrado_DeveLancarDomainException()
        {
            var dto = new SensorDTO
            {
                Id = Guid.NewGuid(),
                Name = "Temperatura",
                Type = "1",
                Value = 30.5,
                Timestamp = DateTime.UtcNow
            };

            _sensorRepositoryMock
                .Setup(x => x.GetByIdAsync(dto.Id))
                .ReturnsAsync((Sensor?)null);

            Func<Task> act = async () => await _sensorService.UpdateAsync(dto);

            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("Sensor não encontrado.");

            _sensorRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Sensor>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_NomeVazio_DeveLancarDomainException()
        {
            var sensor = new Sensor
            {
                Id = Guid.NewGuid(),
                Name = "Temperatura Antiga",
                Type = 1,
                Value = 20,
                Timestamp = DateTime.UtcNow.AddMinutes(-10)
            };

            var dto = new SensorDTO
            {
                Id = sensor.Id,
                Name = "",
                Type = "1",
                Value = 30.5,
                Timestamp = DateTime.UtcNow
            };

            _sensorRepositoryMock
                .Setup(x => x.GetByIdAsync(sensor.Id))
                .ReturnsAsync(sensor);

            Func<Task> act = async () => await _sensorService.UpdateAsync(dto);

            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("O nome do sensor não pode ser vazio.");

            _sensorRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Sensor>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_TipoVazio_DeveLancarDomainException()
        {
            var sensor = new Sensor
            {
                Id = Guid.NewGuid(),
                Name = "Temperatura",
                Type = 1,
                Value = 20,
                Timestamp = DateTime.UtcNow.AddMinutes(-10)
            };

            var dto = new SensorDTO
            {
                Id = sensor.Id,
                Name = "Temperatura Nova",
                Type = "",
                Value = 30.5,
                Timestamp = DateTime.UtcNow
            };

            _sensorRepositoryMock
                .Setup(x => x.GetByIdAsync(sensor.Id))
                .ReturnsAsync(sensor);

            Func<Task> act = async () => await _sensorService.UpdateAsync(dto);

            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("O tipo do sensor não pode ser vazio.");

            _sensorRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Sensor>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_TipoNaoNumerico_DeveLancarDomainException()
        {
            var sensor = new Sensor
            {
                Id = Guid.NewGuid(),
                Name = "Temperatura",
                Type = 1,
                Value = 20,
                Timestamp = DateTime.UtcNow.AddMinutes(-10)
            };

            var dto = new SensorDTO
            {
                Id = sensor.Id,
                Name = "Temperatura Nova",
                Type = "abc",
                Value = 30.5,
                Timestamp = DateTime.UtcNow
            };

            _sensorRepositoryMock
                .Setup(x => x.GetByIdAsync(sensor.Id))
                .ReturnsAsync(sensor);

            Func<Task> act = async () => await _sensorService.UpdateAsync(dto);

            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("O tipo do sensor deve ser numérico.");

            _sensorRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Sensor>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_DadosValidos_DeveAtualizarSensor()
        {
            var sensor = new Sensor
            {
                Id = Guid.NewGuid(),
                Name = "Temperatura Antiga",
                Type = 1,
                Value = 20,
                Timestamp = new DateTime(2026, 4, 7, 9, 0, 0, DateTimeKind.Utc),
                CreatedAt = new DateTime(2026, 4, 7, 8, 0, 0, DateTimeKind.Utc)
            };

            var novoTimestamp = new DateTime(2026, 4, 7, 11, 0, 0, DateTimeKind.Utc);

            var dto = new SensorDTO
            {
                Id = sensor.Id,
                Name = "Temperatura Nova",
                Type = "3",
                Value = 30.5,
                Timestamp = novoTimestamp
            };

            Sensor? sensorAtualizado = null;

            _sensorRepositoryMock
                .Setup(x => x.GetByIdAsync(sensor.Id))
                .ReturnsAsync(sensor);

            _sensorRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Sensor>()))
                .Callback<Sensor>(s => sensorAtualizado = s)
                .Returns(Task.CompletedTask);

            await _sensorService.UpdateAsync(dto);

            _sensorRepositoryMock.Verify(x => x.GetByIdAsync(sensor.Id), Times.Once);
            _sensorRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Sensor>()), Times.Once);

            sensorAtualizado.Should().NotBeNull();
            sensorAtualizado!.Name.Should().Be("Temperatura Nova");
            sensorAtualizado.Type.Should().Be(3);
            sensorAtualizado.Value.Should().Be(30.5);
            sensorAtualizado.Timestamp.Should().Be(novoTimestamp);
            sensorAtualizado.UpdatedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task DeleteAsync_SensorNaoEncontrado_DeveLancarDomainException()
        {
            var id = Guid.NewGuid();

            _sensorRepositoryMock
                .Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync((Sensor?)null);

            Func<Task> act = async () => await _sensorService.DeleteAsync(id);

            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("Sensor não encontrado.");

            _sensorRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_SensorExistente_DeveRemoverSensor()
        {
            var id = Guid.NewGuid();

            var sensor = new Sensor
            {
                Id = id,
                Name = "Temperatura",
                Type = 1,
                Value = 25.4,
                Timestamp = DateTime.UtcNow
            };

            _sensorRepositoryMock
                .Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(sensor);

            _sensorRepositoryMock
                .Setup(x => x.DeleteAsync(id))
                .Returns(Task.CompletedTask);

            await _sensorService.DeleteAsync(id);

            _sensorRepositoryMock.Verify(x => x.GetByIdAsync(id), Times.Once);
            _sensorRepositoryMock.Verify(x => x.DeleteAsync(id), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_SensorNaoEncontrado_DeveRetornarNull()
        {
            var id = Guid.NewGuid();

            _sensorRepositoryMock
                .Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync((Sensor?)null);

            var result = await _sensorService.GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_SensorExistente_DeveRetornarDto()
        {
            var id = Guid.NewGuid();
            var timestamp = new DateTime(2026, 4, 7, 12, 0, 0, DateTimeKind.Utc);

            var sensor = new Sensor
            {
                Id = id,
                Name = "Temperatura",
                Type = 1,
                Value = 25.4,
                Timestamp = timestamp
            };

            _sensorRepositoryMock
                .Setup(x => x.GetByIdAsync(id))
                .ReturnsAsync(sensor);

            var result = await _sensorService.GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.Name.Should().Be("Temperatura");
            result.Type.Should().Be("1");
            result.Value.Should().Be(25.4);
            result.Timestamp.Should().Be(timestamp);
        }

        [Fact]
        public async Task GetAllAsync_QuandoExistiremSensores_DeveRetornarListaMapeada()
        {
            var sensores = new List<Sensor>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Temperatura",
                    Type = 1,
                    Value = 25.4,
                    Timestamp = new DateTime(2026, 4, 7, 10, 0, 0, DateTimeKind.Utc)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Umidade",
                    Type = 2,
                    Value = 60,
                    Timestamp = new DateTime(2026, 4, 7, 10, 5, 0, DateTimeKind.Utc)
                }
            };

            _sensorRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(sensores);

            var result = (await _sensorService.GetAllAsync()).ToList();

            result.Should().HaveCount(2);

            result[0].Id.Should().Be(sensores[0].Id);
            result[0].Name.Should().Be("Temperatura");
            result[0].Type.Should().Be("1");
            result[0].Value.Should().Be(25.4);

            result[1].Id.Should().Be(sensores[1].Id);
            result[1].Name.Should().Be("Umidade");
            result[1].Type.Should().Be("2");
            result[1].Value.Should().Be(60);
        }

        [Fact]
        public async Task SearchAsync_SemFiltros_DeveRetornarResultadoPaginado()
        {
            var sensores = CriarSensoresParaBusca();

            _sensorRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(sensores);

            var searchDto = new SensorSearchDTO
            {
                Page = 1,
                PageSize = 2
            };

            var result = await _sensorService.SearchAsync(searchDto);

            result.Should().NotBeNull();
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(2);
            result.TotalItems.Should().Be(4);
            result.TotalPages.Should().Be(2);
            result.Items.Should().HaveCount(2);
        }

        [Fact]
        public async Task SearchAsync_FiltroPorNome_DeveRetornarSomenteItensCorrespondentes()
        {
            var sensores = CriarSensoresParaBusca();

            _sensorRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(sensores);

            var searchDto = new SensorSearchDTO
            {
                Name = "temp",
                Page = 1,
                PageSize = 10
            };

            var result = await _sensorService.SearchAsync(searchDto);

            result.TotalItems.Should().Be(2);
            result.Items.Should().HaveCount(2);
            result.Items.All(x => x.Name.Contains("Temp", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
        }

        [Fact]
        public async Task SearchAsync_FiltroPorTipo_DeveRetornarSomenteItensDoTipoInformado()
        {
            var sensores = CriarSensoresParaBusca();

            _sensorRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(sensores);

            var searchDto = new SensorSearchDTO
            {
                Type = "2",
                Page = 1,
                PageSize = 10
            };

            var result = await _sensorService.SearchAsync(searchDto);

            result.TotalItems.Should().Be(2);
            result.Items.Should().HaveCount(2);
            result.Items.All(x => x.Type == "2").Should().BeTrue();
        }

        [Fact]
        public async Task SearchAsync_FiltroPorMinValue_DeveRetornarItensComValorMaiorOuIgual()
        {
            var sensores = CriarSensoresParaBusca();

            _sensorRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(sensores);

            var searchDto = new SensorSearchDTO
            {
                MinValue = 40,
                Page = 1,
                PageSize = 10
            };

            var result = await _sensorService.SearchAsync(searchDto);

            result.TotalItems.Should().Be(1);
            result.Items.Should().HaveCount(1);
            result.Items.All(x => x.Value >= 40).Should().BeTrue();
            result.Items.First().Value.Should().Be(60);
        }

        [Fact]
        public async Task SearchAsync_FiltroPorMaxValue_DeveRetornarItensComValorMenorOuIgual()
        {
            var sensores = CriarSensoresParaBusca();

            _sensorRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(sensores);

            var searchDto = new SensorSearchDTO
            {
                MaxValue = 25,
                Page = 1,
                PageSize = 10
            };

            var result = await _sensorService.SearchAsync(searchDto);

            result.TotalItems.Should().Be(1);
            result.Items.Should().HaveCount(1);
            result.Items.All(x => x.Value <= 25).Should().BeTrue();
            result.Items.First().Value.Should().Be(6.5);
        }

        [Fact]
        public async Task SearchAsync_FiltroPorPeriodo_DeveRetornarItensDentroDoIntervalo()
        {
            var sensores = CriarSensoresParaBusca();

            _sensorRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(sensores);

            var inicio = new DateTime(2026, 4, 7, 10, 30, 0, DateTimeKind.Utc);
            var fim = new DateTime(2026, 4, 7, 12, 30, 0, DateTimeKind.Utc);

            var searchDto = new SensorSearchDTO
            {
                StartTimestamp = inicio,
                EndTimestamp = fim,
                Page = 1,
                PageSize = 10
            };

            var result = await _sensorService.SearchAsync(searchDto);

            result.TotalItems.Should().Be(2);
            result.Items.Should().HaveCount(2);
            result.Items.All(x => x.Timestamp >= inicio && x.Timestamp <= fim).Should().BeTrue();
        }

        [Fact]
        public async Task SearchAsync_OrdenacaoPorNameAsc_DeveOrdenarCorretamente()
        {
            var sensores = CriarSensoresParaBusca();

            _sensorRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(sensores);

            var searchDto = new SensorSearchDTO
            {
                OrderBy = "name",
                Direction = "asc",
                Page = 1,
                PageSize = 10
            };

            var result = await _sensorService.SearchAsync(searchDto);
            var items = result.Items.ToList();

            items.Should().HaveCount(4);
            items[0].Name.Should().Be("Ph Solo");
            items[1].Name.Should().Be("Temperatura Estufa");
            items[2].Name.Should().Be("Temperatura Solo");
            items[3].Name.Should().Be("Umidade");
        }

        [Fact]
        public async Task SearchAsync_OrdenacaoPorValueDesc_DeveOrdenarCorretamente()
        {
            var sensores = CriarSensoresParaBusca();

            _sensorRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(sensores);

            var searchDto = new SensorSearchDTO
            {
                OrderBy = "value",
                Direction = "desc",
                Page = 1,
                PageSize = 10
            };

            var result = await _sensorService.SearchAsync(searchDto);
            var items = result.Items.ToList();

            items.Should().HaveCount(4);
            items[0].Value.Should().Be(60);
            items[1].Value.Should().Be(35);
            items[2].Value.Should().Be(25.4);
            items[3].Value.Should().Be(6.5);
        }

        [Fact]
        public async Task SearchAsync_Paginacao_DeveRetornarPaginaCorreta()
        {
            var sensores = CriarSensoresParaBusca();

            _sensorRepositoryMock
                .Setup(x => x.GetAllAsync())
                .ReturnsAsync(sensores);

            var searchDto = new SensorSearchDTO
            {
                OrderBy = "timestamp",
                Direction = "desc",
                Page = 2,
                PageSize = 2
            };

            var result = await _sensorService.SearchAsync(searchDto);
            var items = result.Items.ToList();

            result.Page.Should().Be(2);
            result.PageSize.Should().Be(2);
            result.TotalItems.Should().Be(4);
            result.TotalPages.Should().Be(2);
            items.Should().HaveCount(2);

            items[0].Name.Should().Be("Umidade");
            items[1].Name.Should().Be("Temperatura Solo");
        }

        private static List<Sensor> CriarSensoresParaBusca()
        {
            return new List<Sensor>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Temperatura Solo",
                    Type = 1,
                    Value = 25.4,
                    Timestamp = new DateTime(2026, 4, 7, 10, 0, 0, DateTimeKind.Utc)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Umidade",
                    Type = 2,
                    Value = 60,
                    Timestamp = new DateTime(2026, 4, 7, 11, 0, 0, DateTimeKind.Utc)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Ph Solo",
                    Type = 3,
                    Value = 6.5,
                    Timestamp = new DateTime(2026, 4, 7, 12, 0, 0, DateTimeKind.Utc)
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Temperatura Estufa",
                    Type = 2,
                    Value = 35,
                    Timestamp = new DateTime(2026, 4, 7, 13, 0, 0, DateTimeKind.Utc)
                }
            };
        }
    }
}