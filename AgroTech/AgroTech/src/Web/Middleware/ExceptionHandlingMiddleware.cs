using System.Net;
using AgroTech.Application.Exceptions;
using System.Text.Json;

namespace AgroTech.Web.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (DomainException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Erro de domínio tratado. CorrelationId: {CorrelationId}. Message: {Message}",
                    httpContext.TraceIdentifier,
                    ex.Message);

                await HandleExceptionAsync(
                    httpContext,
                    ex.Message,
                    HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro interno não tratado. CorrelationId: {CorrelationId}",
                    httpContext.TraceIdentifier);

                await HandleExceptionAsync(
                    httpContext,
                    "Erro interno no servidor.",
                    HttpStatusCode.InternalServerError);
            }
        }

        private static Task HandleExceptionAsync(
            HttpContext context,
            string message,
            HttpStatusCode statusCode)
        {
            context.Response.ContentType = "application/json; charset=utf-8";
            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                error = message,
                correlationId = context.TraceIdentifier
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            return context.Response.WriteAsync(json);
        }
    }
}