using ECommerceApp.Analytics.Consumers;
using ECommerceApp.Analytics.Data;
using ECommerceApp.Analytics.Repositories.Implementations;
using ECommerceApp.Analytics.Repositories.Interfaces;
using ECommerceApp.Analytics.Services.Implementations;
using ECommerceApp.Analytics.Services.Interfaces;
using ECommerceApp.Shared.Helpers;
using ECommerceApp.Shared.Middleware;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers ────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Database — SQL Server Express ──────────────────────────────
builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration
            .GetConnectionString("Default"),
        sql => sql.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null)));

// ── Repositories ───────────────────────────────────────────────
builder.Services
    .AddScoped<IAnalyticsRepository,
        AnalyticsRepository>();

// ── Services ───────────────────────────────────────────────────
builder.Services
    .AddScoped<IAnalyticsService, AnalyticsService>();

// ── JWT Authentication ─────────────────────────────────────────
builder.Services
    .AddJwtAuthentication(builder.Configuration);

// ── RabbitMQ + MassTransit ─────────────────────────────────────
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderPlacedConsumer>(cfg =>
        cfg.UseMessageRetry(r =>
            r.Intervals(
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(15),
                TimeSpan.FromSeconds(30))));

    x.AddConsumer<PaymentProcessedConsumer>(cfg =>
        cfg.UseMessageRetry(r =>
            r.Intervals(
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(15),
                TimeSpan.FromSeconds(30))));

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
        .GetRequiredService<AnalyticsDbContext>();
    db.Database.Migrate();
}

app.Run();