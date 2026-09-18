using ECommerceApp.Shared.Middleware;
using IdentityServiceAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddDatabaseServices(builder.Configuration);
builder.Services.AddIdentityServices();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorizationServices();
builder.Services.AddSwaggerServices();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddCorsServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("ReactPolicy");

// Rejects any request that did not come through the API Gateway.
// Must run before authentication so a bypass attempt never
// reaches token validation at all.
app.UseMiddleware<RequireGatewayMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();