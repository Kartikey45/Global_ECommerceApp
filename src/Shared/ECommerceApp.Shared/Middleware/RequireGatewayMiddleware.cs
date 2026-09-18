using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace ECommerceApp.Shared.Middleware
{
    // Rejects any request that does not carry the shared secret the
    // API Gateway stamps on every request it proxies. This forces
    // every client to go through the gateway instead of calling a
    // service directly on its own port.
    public class RequireGatewayMiddleware
    {
        public const string HeaderName = "X-Gateway-Secret";

        private readonly RequestDelegate _next;
        private readonly string _expectedSecret;
        private readonly ILogger<RequireGatewayMiddleware> _logger;

        public RequireGatewayMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            ILogger<RequireGatewayMiddleware> logger)
        {
            _next = next;
            _logger = logger;

            _expectedSecret = configuration["Gateway:Secret"]
                ?? throw new InvalidOperationException(
                    "Gateway:Secret is not configured. Every service " +
                    "behind the gateway must define it, or no request " +
                    "would ever be accepted.");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var provided = context.Request.Headers[HeaderName].ToString();

            if (!FixedTimeEquals(provided, _expectedSecret))
            {
                _logger.LogWarning(
                    "Rejected request bypassing the gateway — " +
                    "{Method} {Path} from {IP}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Connection.RemoteIpAddress);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync(
                    "Direct access is not permitted. " +
                    "Call this API through the gateway.");
                return;
            }

            await _next(context);
        }

        private static bool FixedTimeEquals(string a, string b)
        {
            var bytesA = Encoding.UTF8.GetBytes(a);
            var bytesB = Encoding.UTF8.GetBytes(b);

            if (bytesA.Length != bytesB.Length)
                return false;

            return CryptographicOperations.FixedTimeEquals(bytesA, bytesB);
        }
    }
}
