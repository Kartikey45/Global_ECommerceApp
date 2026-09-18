using ECommerceApp.Shared.Middleware;

namespace ECommerceApp.ApiGateway.Middleware
{
    // Stamps every request the gateway proxies with the shared
    // secret so downstream services can verify it did not arrive
    // by a direct, gateway-bypassing call. Runs before Ocelot so
    // the header is present on every route without repeating it
    // in ocelot.json for each one.
    public class GatewaySecretHeaderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _secret;

        public GatewaySecretHeaderMiddleware(
            RequestDelegate next,
            IConfiguration configuration)
        {
            _next = next;

            _secret = configuration["Gateway:Secret"]
                ?? throw new InvalidOperationException(
                    "Gateway:Secret is not configured for the API Gateway.");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.Headers[RequireGatewayMiddleware.HeaderName] = _secret;

            await _next(context);
        }
    }
}
