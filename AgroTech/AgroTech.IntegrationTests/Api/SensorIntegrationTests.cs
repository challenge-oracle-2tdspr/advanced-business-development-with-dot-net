using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace AgroTech.IntegrationTests.Api
{
    [Collection("Integration Test Collection")]
    public class SensorsIntegrationTests
    {
        private readonly HttpClient _client;

        public SensorsIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAllAsync_QuandoExistiremSensores_DeveRetornarOkELista()
        {
            // Arrange

            // Act
            var response = await _client.GetAsync("/api/sensors");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadFromJsonAsync<List<SensorResponse>>();
            content.Should().NotBeNull();
            content.Should().NotBeEmpty();
            content!.Should().HaveCountGreaterThanOrEqualTo(3);
            content.Should().Contain(x => x.Name == "Temperatura");
            content.Should().Contain(x => x.Name == "Umidade");
        }

        [Fact]
        public async Task GetByIdAsync_QuandoSensorExistir_DeveRetornarOk()
        {
            // Arrange
            var sensorId = "11111111-1111-1111-1111-111111111111";

            // Act
            var response = await _client.GetAsync($"/api/sensors/{sensorId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadFromJsonAsync<SensorResponse>();
            content.Should().NotBeNull();
            content!.Id.Should().Be(Guid.Parse(sensorId));
            content.Type.Should().Be("1");
            content.Links.Should().NotBeNullOrEmpty();
            content.Links.Should().Contain(x => x.Rel == "self");
            content.Links.Should().Contain(x => x.Rel == "update");
            content.Links.Should().Contain(x => x.Rel == "delete");
            content.Links.Should().Contain(x => x.Rel == "search");
        }
        
        [Fact]
        public async Task GetByIdAsync_QuandoSensorNaoExistir_DeveRetornarNotFound()
        {
            // Arrange
            var sensorId = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/sensors/{sensorId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task SearchAsync_ComFiltroPorNome_DeveRetornarResultadoPaginado()
        {
            // Arrange
            var url = "/api/sensors/search?name=temp&page=1&pageSize=10";

            // Act
            var response = await _client.GetAsync(url);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadFromJsonAsync<PagedResultResponse<SensorResponse>>();
            content.Should().NotBeNull();
            content!.Items.Should().NotBeNullOrEmpty();
            content.Items.Should().OnlyContain(x => x.Name.Contains("Temp", StringComparison.OrdinalIgnoreCase));
            content.Page.Should().Be(1);
            content.PageSize.Should().Be(10);
            content.TotalItems.Should().BeGreaterThan(0);
            content.TotalPages.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_ComDadosValidos_DeveRetornarOkEIdsCriados()
        {
            // Arrange
            var request = new List<CreateSensorRequest>
            {
                new()
                {
                    Name = "Luminosidade",
                    Type = "4",
                    Value = 800,
                    Timestamp = DateTime.UtcNow
                },
                new()
                {
                    Name = "Chuva",
                    Type = "5",
                    Value = 12.3,
                    Timestamp = DateTime.UtcNow
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/sensors", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadFromJsonAsync<CreateSensorsResponse>();
            content.Should().NotBeNull();
            content!.Message.Should().Be("Sensores criados com sucesso.");
            content.Ids.Should().HaveCount(2);
            content.Ids.Should().OnlyHaveUniqueItems();
        }

        [Fact]
        public async Task CreateAsync_ComListaVazia_DeveRetornarBadRequestOuInternalServerErrorTratado()
        {
            // Arrange
            var request = new List<CreateSensorRequest>();

            // Act
            var response = await _client.PostAsJsonAsync("/api/sensors", request);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task UpdateAsync_QuandoDadosForemValidos_DeveRetornarNoContent()
        {
            // Arrange
            var request = new UpdateSensorRequest
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Temperatura Atualizada",
                Type = "1",
                Value = 28.9,
                Timestamp = DateTime.UtcNow
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/sensors/{request.Id}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await _client.GetAsync($"/api/sensors/{request.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var updated = await getResponse.Content.ReadFromJsonAsync<SensorResponse>();
            updated.Should().NotBeNull();
            updated!.Name.Should().Be("Temperatura Atualizada");
            updated.Value.Should().Be(28.9);
        }

        [Fact]
        public async Task UpdateAsync_QuandoIdDaRotaForDiferenteDoCorpo_DeveRetornarBadRequest()
        {
            // Arrange
            var request = new UpdateSensorRequest
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Temperatura Atualizada",
                Type = "1",
                Value = 28.9,
                Timestamp = DateTime.UtcNow
            };

            var routeId = Guid.Parse("99999999-9999-9999-9999-999999999999");

            // Act
            var response = await _client.PutAsJsonAsync($"/api/sensors/{routeId}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task DeleteAsync_QuandoSensorExistir_DeveRetornarNoContent()
        {
            // Arrange
            var sensorId = "22222222-2222-2222-2222-222222222222";

            // Act
            var response = await _client.DeleteAsync($"/api/sensors/{sensorId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            var getResponse = await _client.GetAsync($"/api/sensors/{sensorId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        private sealed class SensorResponse
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public double Value { get; set; }
            public DateTime Timestamp { get; set; }
            public List<LinkResponse> Links { get; set; } = new();
        }

        private sealed class LinkResponse
        {
            public string Rel { get; set; } = string.Empty;
            public string Href { get; set; } = string.Empty;
            public string Method { get; set; } = string.Empty;
        }

        private sealed class PagedResultResponse<T>
        {
            public IEnumerable<T> Items { get; set; } = new List<T>();
            public int Page { get; set; }
            public int PageSize { get; set; }
            public int TotalItems { get; set; }
            public int TotalPages { get; set; }
        }

        private sealed class CreateSensorRequest
        {
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public double Value { get; set; }
            public DateTime Timestamp { get; set; }
        }

        private sealed class UpdateSensorRequest
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public double Value { get; set; }
            public DateTime Timestamp { get; set; }
        }

        private sealed class CreateSensorsResponse
        {
            public string Message { get; set; } = string.Empty;
            public List<Guid> Ids { get; set; } = new();
        }
    }
}