using System.Net;
using AgroTech.Application.Exceptions;
using System.Text.Json;

namespace AgroTech.Web.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        public ExceptionHandlingMiddleware(RequestDelegate next,  ILogger<ExceptionHandlingMiddleware> logger)
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
                _logger.LogWarning(ex, "Erro de domínio tratado: {Message}", ex.Message);
                await HandleExceptionAsync(httpContext, ex.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno não tratado.");
                await HandleExceptionAsync(httpContext, "Erro interno no servidor.", HttpStatusCode.InternalServerError);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, string message, HttpStatusCode statusCode)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new { error = message };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}