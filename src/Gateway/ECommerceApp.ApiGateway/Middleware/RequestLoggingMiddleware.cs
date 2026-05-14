namespace ECommerceApp.ApiGateway.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware>
            _logger;

        public RequestLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var start = DateTime.UtcNow;

            _logger.LogInformation(
                "Gateway → {Method} {Path} | " +
                "IP: {IP}",
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress);

            await _next(context);

            var elapsed =
                (DateTime.UtcNow - start)
                .TotalMilliseconds;

            _logger.LogInformation(
                "Gateway ← {Method} {Path} | " +
                "Status: {StatusCode} | " +
                "{Elapsed}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                elapsed);
        }
    }
}