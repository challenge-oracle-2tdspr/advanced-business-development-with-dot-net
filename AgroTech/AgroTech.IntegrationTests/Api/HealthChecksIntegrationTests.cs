using System.Net;
using System.Text.Json;
using FluentAssertions;

namespace AgroTech.IntegrationTests.Api
{
    [Collection("Integration Test Collection")]
    public class HealthChecksIntegrationTests
    {
        private readonly HttpClient _client;

        public HealthChecksIntegrationTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Health_DeveRetornarOkEStatusHealthy()
        {
            // Arrange

            // Act
            var response = await _client.GetAsync("/health");
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine(body);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            using var json = JsonDocument.Parse(content);

            json.RootElement.GetProperty("status").GetString().Should().Be("Healthy");
            json.RootElement.TryGetProperty("totalDurationMs", out _).Should().BeTrue();
            json.RootElement.TryGetProperty("checks", out var checks).Should().BeTrue();
            checks.ValueKind.Should().Be(JsonValueKind.Array);
        }

        [Fact]
        public async Task HealthLive_DeveRetornarOkEStatusHealthy()
        {
            // Arrange

            // Act
            var response = await _client.GetAsync("/health/live");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            using var json = JsonDocument.Parse(content);

            json.RootElement.GetProperty("status").GetString().Should().Be("Healthy");
        }

        [Fact]
        public async Task HealthReady_DeveRetornarOkEStatusHealthy()
        {
            // Arrange

            // Act
            var response = await _client.GetAsync("/health/ready");
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine(body);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            using var json = JsonDocument.Parse(content);

            json.RootElement.GetProperty("status").GetString().Should().Be("Healthy");
        }
    }
}