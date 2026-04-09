using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace AgroTech.Messaging
{
    public class HttpCorrelationIdAccessor : ICorrelationIdAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpCorrelationIdAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCorrelationId()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var correlationIdFromResponse = httpContext?
                .Response
                .Headers["X-Correlation-ID"]
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(correlationIdFromResponse))
                return correlationIdFromResponse!;

            var correlationIdFromRequest = httpContext?
                .Request
                .Headers["X-Correlation-ID"]
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(correlationIdFromRequest))
                return correlationIdFromRequest!;

            var activityTraceId = Activity.Current?.TraceId.ToString();

            if (!string.IsNullOrWhiteSpace(activityTraceId))
                return activityTraceId!;

            return Guid.NewGuid().ToString();
        }
    }
}