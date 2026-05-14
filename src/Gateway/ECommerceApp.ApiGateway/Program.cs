using ECommerceApp.ApiGateway.Middleware;
using ECommerceApp.Shared.Helpers;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ── Load ocelot.json ───────────────────────────────────────────
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile("ocelot.json", false, true)
    .AddEnvironmentVariables();

// ── JWT Authentication ─────────────────────────────────────────
// Same config as Identity Service
// Gateway validates token before forwarding
builder.Services
    .AddJwtAuthentication(builder.Configuration);

// ── CORS ───────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",  // Angular Customer
                "http://localhost:4201")  // Angular Admin
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// ── Ocelot ─────────────────────────────────────────────────────
builder.Services.AddOcelot(builder.Configuration);

// ── Swagger (Gateway level docs) ───────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ── Middleware Pipeline ────────────────────────────────────────
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseCors("AllowAngular");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

// ── Ocelot handles all routing ─────────────────────────────────
// Must be last in pipeline
await app.UseOcelot();

app.Run();