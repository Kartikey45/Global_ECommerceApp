using ECommerceApp.Product.Data;
using ECommerceApp.Product.Repositories.Implementations;
using ECommerceApp.Product.Repositories.Interfaces;
using ECommerceApp.Product.Services.Implementations;
using ECommerceApp.Product.Services.Interfaces;
using ECommerceApp.Shared.Helpers;
using ECommerceApp.Shared.Middleware;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// ── Controllers ────────────────────────────────────────────────
builder.Services.AddControllers();

// ── Database — SQL Server Express ──────────────────────────────
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration
            .GetConnectionString("Default"),
        sql => sql.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null)));

// ── Distributed Redis Cache (Memurai) ──────────────────────────
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration =
        builder.Configuration["Redis:ConnectionString"];
    options.InstanceName =
        builder.Configuration["Redis:InstanceName"];
});

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(
        builder.Configuration["Redis:ConnectionString"]!));

// ── Repositories ───────────────────────────────────────────────
builder.Services
    .AddScoped<IProductRepository, ProductRepository>();
builder.Services
    .AddScoped<ICategoryRepository, CategoryRepository>();

// ── Services ───────────────────────────────────────────────────
builder.Services
    .AddScoped<IProductService, ProductService>();
builder.Services
    .AddScoped<ICategoryService, CategoryService>();
builder.Services
    .AddSingleton<ICacheService, DistributedCacheService>();

// ── JWT Authentication ─────────────────────────────────────────
builder.Services
    .AddJwtAuthentication(builder.Configuration);

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

// ── Auto Migrate + Seed on Startup ────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<ProductDbContext>();
    db.Database.Migrate();
}

app.Run();