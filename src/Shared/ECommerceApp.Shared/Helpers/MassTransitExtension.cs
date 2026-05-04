using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceApp.Shared.Helpers
{
    public static class MassTransitExtension
    {
        public static IServiceCollection AddRabbitMqBus(
            this IServiceCollection services,
            IConfiguration config,
            Action<IBusRegistrationConfigurator>?
                registerConsumers = null)
        {
            services.AddMassTransit(x =>
            {
                // Caller registers their consumers
                registerConsumers?.Invoke(x);

                x.UsingRabbitMq((ctx, cfg) =>
                {
                    cfg.Host(
                        config["RabbitMQ:Host"] ?? "localhost",
                        config["RabbitMQ:VHost"] ?? "/",
                        h =>
                        {
                            h.Username(
                                config["RabbitMQ:Username"]
                                ?? "guest");
                            h.Password(
                                config["RabbitMQ:Password"]
                                ?? "guest");
                        });

                    // ── Global retry policy ────────────────
                    cfg.UseMessageRetry(r =>
                        r.Intervals(
                            TimeSpan.FromSeconds(5),
                            TimeSpan.FromSeconds(15),
                            TimeSpan.FromSeconds(30)));

                    // Auto-creates queues + _error DLQ queues
                    cfg.ConfigureEndpoints(ctx);
                });
            });

            return services;
        }
    }
}