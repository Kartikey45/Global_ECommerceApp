using ECommerceApp.Order.Consumers;
using ECommerceApp.Order.Data;
using ECommerceApp.Order.HttpClients;
using ECommerceApp.Order.Repositories.Implementations;
using ECommerceApp.Order.Repositories.Interfaces;
using ECommerceApp.Order.Services.Implementations;
using ECommerceApp.Order.Services.Interfaces;
using ECommerceApp.Shared.Helpers;
using ECommerceApp.Shared.Middleware;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers ────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Database — SQL Server Express ──────────────────────────────
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration
            .GetConnectionString("Default"),
        sql => sql.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null)));

// ── Product Service HTTP Client ────────────────────────────────
builder.Services.AddHttpClient<ProductServiceClient>(
    client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration[
                "ProductService:BaseUrl"]!);
    });

// ── Repositories ───────────────────────────────────────────────
builder.Services
    .AddScoped<IOrderRepository, OrderRepository>();

// ── Services ───────────────────────────────────────────────────
builder.Services
    .AddScoped<IOrderService, OrderService>();

// ── JWT Authentication ─────────────────────────────────────────
builder.Services
    .AddJwtAuthentication(builder.Configuration);

// ── RabbitMQ + MassTransit ─────────────────────────────────────
builder.Services.AddMassTransit(x =>
{
    // Register consumers
    x.AddConsumer<PaymentProcessedConsumer>(cfg =>
        cfg.UseMessageRetry(r =>
            r.Intervals(
                TimeSpan.FromSeconds(5),
                TimeSpan.FromSeconds(15),
                TimeSpan.FromSeconds(30))));

    x.AddConsumer<PaymentProcessedFaultConsumer>();
    x.AddConsumer<OrderShippedConsumer>();

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
        .GetRequiredService<OrderDbContext>();
    db.Database.Migrate();
}

app.Run();