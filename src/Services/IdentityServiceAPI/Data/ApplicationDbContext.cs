using IdentityServiceAPI.Models;
using IdentityServiceAPI.Models.EntityModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityServiceAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> option) : base(option) { }



        public DbSet<Permissions> Permissions { get; set; }
        public DbSet<RolePermissions> RolePermissions { get; set; }
        public DbSet<RevokedToken> RevokedTokens { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Logout writes one row per token; every authenticated
            // request then looks the jti up, so it must be indexed.
            builder.Entity<RevokedToken>()
                .HasIndex(t => t.Jti)
                .IsUnique();
        }
    }


}
