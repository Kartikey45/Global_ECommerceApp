using ECommerceApp.Shared.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace ECommerceApp.Shared.Middleware
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

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unhandled exception: {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                await HandleAsync(context, ex);
            }
        }

        private static Task HandleAsync(
            HttpContext ctx, Exception ex)
        {
            ctx.Response.ContentType = "application/json";

            var (code, message) = ex switch
            {
                UnauthorizedAccessException =>
                    (HttpStatusCode.Unauthorized,
                        "Unauthorized access."),
                KeyNotFoundException =>
                    (HttpStatusCode.NotFound,
                        ex.Message),
                ArgumentException =>
                    (HttpStatusCode.BadRequest,
                        ex.Message),
                InvalidOperationException =>
                    (HttpStatusCode.BadRequest,
                        ex.Message),
                _ =>
                    (HttpStatusCode.InternalServerError,
                        "An unexpected error occurred.")
            };

            ctx.Response.StatusCode = (int)code;

            return ctx.Response.WriteAsync(
                JsonSerializer.Serialize(
                    ApiResponse<object>.Fail(message),
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy =
                            JsonNamingPolicy.CamelCase
                    }));
        }
    }
}