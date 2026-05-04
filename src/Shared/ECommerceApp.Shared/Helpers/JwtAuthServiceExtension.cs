using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ECommerceApp.Shared.Helpers
{
    public static class JwtAuthServiceExtension
    {
        /// <summary>
        /// Adds JWT authentication to any microservice using the
        /// exact same config keys as IdentityServiceAPI:
        /// Jwt:Key, Jwt:Issuer, Jwt:Audience
        /// </summary>
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ── Matches IdentityServiceAPI ServiceExtensions ──
            var jwtKey = configuration["Jwt:Key"];
            var jwtIssuer = configuration["Jwt:Issuer"];
            var jwtAudience = configuration["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(jwtKey) ||
                string.IsNullOrWhiteSpace(jwtIssuer) ||
                string.IsNullOrWhiteSpace(jwtAudience))
            {
                throw new InvalidOperationException(
                    "JWT configuration is missing. " +
                    "Ensure Jwt:Key, Jwt:Issuer, and " +
                    "Jwt:Audience are set.");
            }

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme =
                        JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme =
                        JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,

                            ValidIssuer = jwtIssuer,
                            ValidAudience = jwtAudience,
                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(
                                        jwtKey)),

                            // Matches Identity Svc:
                            // token expires = DateTime.Now.AddHours(2)
                            // No ClockSkew set in Identity Svc
                            // so we keep it default (5 min tolerance)
                        };

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = ctx =>
                        {
                            if (ctx.Exception
                                is SecurityTokenExpiredException)
                            {
                                ctx.Response.Headers
                                    .Append(
                                        "Token-Expired",
                                        "true");
                            }
                            return Task.CompletedTask;
                        },

                        OnForbidden = ctx =>
                        {
                            ctx.Response.StatusCode = 403;
                            return Task.CompletedTask;
                        }
                    };
                });

            // ── Authorization policies matching your roles ────
            services.AddAuthorization(options =>
            {
                // Matches: [Authorize(Roles = "Admin")]
                options.AddPolicy("AdminOnly",
                    p => p.RequireRole("admin"));

                // For customer-facing endpoints
                options.AddPolicy("CustomerOnly",
                    p => p.RequireRole("customer"));

                // Either role can access
                options.AddPolicy("CustomerOrAdmin",
                    p => p.RequireRole("customer", "admin"));

                // ── Permission-based policies ─────────────────
                // Matches your DynamicPermissionAttribute logic
                options.AddPolicy("CanView",
                    p => p.RequireClaim("Permission", "View"));
                options.AddPolicy("CanCreate",
                    p => p.RequireClaim("Permission", "Create"));
                options.AddPolicy("CanEdit",
                    p => p.RequireClaim("Permission", "Edit"));
                options.AddPolicy("CanDelete",
                    p => p.RequireClaim("Permission", "Delete"));
            });

            return services;
        }
    }
}