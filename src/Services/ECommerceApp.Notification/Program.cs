using ECommerceApp.Notification.Consumers;
using ECommerceApp.Notification.Services.Implementations;
using ECommerceApp.Notification.Services.Interfaces;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);

// ── Email Service (Mailhog) ────────────────────────────────────
builder.Services
    .AddScoped<IEmailService, EmailService>();

// ── RabbitMQ + MassTransit ─────────────────────────────────────
builder.Services.AddMassTransit(x =>
{
    // Register all consumers
    x.AddConsumer<UserRegisteredConsumer>();
    x.AddConsumer<OrderPlacedConsumer>();
    x.AddConsumer<PaymentProcessedConsumer>();
    x.AddConsumer<OrderShippedConsumer>();
    x.AddConsumer<OrderCancelledConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(
            builder.Configuration["RabbitMQ:Host"]
                ?? "localhost",
            builder.Configuration["RabbitMQ:VHost"]
                ?? "/",
            h =>
            {
                h.Username(
                    builder.Configuration[
                        "RabbitMQ:Username"]
                    ?? "guest");
                h.Password(
                    builder.Configuration[
                        "RabbitMQ:Password"]
                    ?? "guest");
            });

        // Global retry — email failures retry 3 times
        cfg.UseMessageRetry(r =>
            r.Intervals(
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(15),
                TimeSpan.FromSeconds(30)));

        // Auto-creates queues + _error DLQ queues
        cfg.ConfigureEndpoints(ctx);
    });
});

var app = builder.Build();
app.Run();