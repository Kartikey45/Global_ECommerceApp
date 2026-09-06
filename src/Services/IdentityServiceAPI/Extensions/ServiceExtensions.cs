using ECommerceApp.Shared.Authorization;
using ECommerceApp.Shared.Constants;
using IdentityServiceAPI.Data;
using IdentityServiceAPI.Models;
using IdentityServiceAPI.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityServiceAPI.Extensions
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Register database and Entity Framework Core services
        /// </summary>
        public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("Default")));

            return services;
        }

        /// <summary>
        /// Register Identity services with custom options
        /// </summary>
        public static IServiceCollection AddIdentityServices(this IServiceCollection services)
        {
            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                options.SignIn.RequireConfirmedEmail = true;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 3;
                /*  
                options.Lockout.AllowedForNewUsers = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
                */
            });

            services.Configure<DataProtectionTokenProviderOptions>(o =>
                o.TokenLifespan = TimeSpan.FromHours(5));

            return services;
        }

        /// <summary>
        /// Register JWT Bearer authentication with validation
        /// </summary>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtKey = configuration["Jwt:Key"];
            var jwtIssuer = configuration["Jwt:Issuer"];
            var jwtAudience = configuration["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(jwtKey) ||
                string.IsNullOrWhiteSpace(jwtIssuer) ||
                string.IsNullOrWhiteSpace(jwtAudience))
            {
                throw new InvalidOperationException(
                    "JWT configuration is missing. Ensure Jwt:Key, Jwt:Issuer, and Jwt:Audience are set in configuration.");
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    //ClockSkew = TimeSpan.FromMinutes(5)
                };

                options.Events = new JwtBearerEvents
                {
                    // A JWT stays cryptographically valid until it expires,
                    // so logout has to be enforced here: reject any token
                    // whose jti was recorded by the logout endpoint.
                    OnTokenValidated = async ctx =>
                    {
                        var jti = ctx.Principal?.FindFirstValue(
                            JwtRegisteredClaimNames.Jti);

                        if (string.IsNullOrEmpty(jti))
                            return;

                        var blocklist = ctx.HttpContext.RequestServices
                            .GetRequiredService<ITokenBlocklistService>();

                        if (await blocklist.IsRevokedAsync(jti))
                        {
                            ctx.Fail("This token has been logged out.");
                        }
                    },

                    OnChallenge = ctx =>
                    {
                        // Surface the reason so Postman/Swagger shows
                        // "logged out" instead of a bare 401.
                        if (ctx.AuthenticateFailure is not null)
                        {
                            ctx.Response.Headers.Append(
                                "Token-Revoked", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }

        /// <summary>
        /// Register authorization services
        /// </summary>
        public static IServiceCollection AddAuthorizationServices(this IServiceCollection services)
        {
            services.AddAuthorization();

            // ── Permission-based authorization ────────────────────────
            // Resolves "Permission:{name}" policies on demand, so
            // [HasPermission("...")] works for any permission in the
            // Permissions table without a startup registration.
            services.AddSingleton<
                IAuthorizationPolicyProvider, PermissionPolicyProvider>();

            services.AddScoped<
                IAuthorizationHandler, PermissionAuthorizationHandler>();

            return services;
        }

        /// <summary>
        /// Register Swagger/OpenAPI services with JWT Bearer configuration
        /// </summary>
        public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token in the format: Bearer {your token}"
                });

                // OpenAPI 3.x (Swashbuckle 10) replaced OpenApiReference
                // with typed reference classes, and AddSecurityRequirement
                // now takes a factory over the document.
                options.AddSecurityRequirement(_ =>
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecuritySchemeReference("Bearer"),
                            new List<string>()
                        }
                    });
            });

            return services;
        }

        /// <summary>
        /// Register application-specific services (User, Email, Token)
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserClaimsPrincipalFactory<User>, ApplicationUserClaimsPricipalFactory>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<ITokenBlocklistService, TokenBlocklistService>();

            services.Configure<SMTPConfigModel>(configuration.GetSection("SMTPConfig"));

            return services;
        }

        /// <summary>
        /// Register CORS policy for React frontend
        /// </summary>
        public static IServiceCollection AddCorsServices(this IServiceCollection services, IConfiguration configuration)
        {
            var reactAppUrl = configuration["CORS:ReactAppUrl"] ?? "http://localhost:3000";

            services.AddCors(options =>
            {
                options.AddPolicy("ReactPolicy", builder =>
                {
                    builder
                        .WithOrigins(reactAppUrl)
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

            return services;
        }
    }
}