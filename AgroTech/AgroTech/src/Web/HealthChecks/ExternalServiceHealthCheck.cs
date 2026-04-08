using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AgroTech.Web.HealthChecks
{
    public class ExternalServiceHealthCheck : IHealthCheck
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ExternalServiceHealthCheck> _logger;

        public ExternalServiceHealthCheck(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<ExternalServiceHealthCheck> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var externalServiceUrl = _configuration["HealthChecks:ExternalServiceUrl"];

            if (string.IsNullOrWhiteSpace(externalServiceUrl))
            {
                _logger.LogWarning("Health check do serviço externo não configurado.");
                return HealthCheckResult.Degraded("URL do serviço externo não configurada.");
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);

                using var response = await client.GetAsync(externalServiceUrl, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "Serviço externo disponível. URL: {Url}, StatusCode: {StatusCode}",
                        externalServiceUrl,
                        (int)response.StatusCode);

                    return HealthCheckResult.Healthy(
                        $"Serviço externo disponível. StatusCode: {(int)response.StatusCode}");
                }

                _logger.LogWarning(
                    "Serviço externo respondeu com falha. URL: {Url}, StatusCode: {StatusCode}",
                    externalServiceUrl,
                    (int)response.StatusCode);

                return HealthCheckResult.Unhealthy(
                    $"Serviço externo indisponível. StatusCode: {(int)response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao verificar serviço externo. URL: {Url}",
                    externalServiceUrl);

                return HealthCheckResult.Unhealthy(
                    "Falha ao conectar no serviço externo.",
                    ex);
            }
        }
    }
}