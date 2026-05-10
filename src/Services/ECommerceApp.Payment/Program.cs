using ECommerceApp.Payment.Consumers;
using ECommerceApp.Payment.Data;
using ECommerceApp.Payment.Repositories.Implementations;
using ECommerceApp.Payment.Repositories.Interfaces;
using ECommerceApp.Payment.Services.Implementations;
using ECommerceApp.Payment.Services.Interfaces;
using ECommerceApp.Shared.Helpers;
using ECommerceApp.Shared.Middleware;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers ────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Database — SQL Server Express ──────────────────────────────
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration
            .GetConnectionString("Default"),
        sql => sql.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null)));

// ── Repositories ───────────────────────────────────────────────
builder.Services
    .AddScoped<IPaymentRepository, PaymentRepository>();

// ── Services ───────────────────────────────────────────────────
builder.Services
    .AddScoped<IPaymentService, PaymentService>();
builder.Services
    .AddScoped<IStripeService, StripeService>();

// ── JWT Authentication ─────────────────────────────────────────
builder.Services
    .AddJwtAuthentication(builder.Configuration);

// ── RabbitMQ + MassTransit ─────────────────────────────────────
builder.Services.AddMassTransit(x =>
{
    // Register OrderPlacedConsumer with retry
    x.AddConsumer<OrderPlacedConsumer>(cfg =>
        cfg.UseMessageRetry(r =>
            r.Intervals(
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(15),
                TimeSpan.FromSeconds(30))));

    // DLQ handler
    x.AddConsumer<OrderPlacedFaultConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(
            builder.Configuration["RabbitMQ:Host"],
            builder.Configuration["RabbitMQ:VHost"],
            h =>
            {
                h.Username(builder.Configuration[
                    "RabbitMQ:Username"]!);
                h.Password(builder.Configuration[
                    "RabbitMQ:Password"]!);
            });

        cfg.ConfigureEndpoints(ctx);
    });
});

// ── Swagger ────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ── Middleware Pipeline ────────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ── Auto Migrate on Startup ────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<PaymentDbContext>();
    db.Database.Migrate();
}

app.Run();