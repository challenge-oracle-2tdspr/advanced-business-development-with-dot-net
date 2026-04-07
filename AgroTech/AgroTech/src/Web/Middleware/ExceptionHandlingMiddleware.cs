using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using AgroTech.Application.Exceptions;
using System.Text.Json;

namespace AgroTech.Web.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (DomainException ex)
            {
                await HandleExceptionAsync(httpContext, ex.Message, HttpStatusCode.BadRequest);
            }
            catch (Exception)
            {
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