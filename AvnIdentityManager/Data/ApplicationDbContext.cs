using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AvnIdentityManager;

namespace AvnIdentityManager.Data;

public class ApplicationDbContext : IdentityDbContext<IMApplicationUser, IMApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IMApplicationUser>()
            .HasMany(p => p.Roles).WithOne()
            .HasForeignKey(p => p.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<IMApplicationUser>()
            .HasMany(e => e.Claims)
            .WithOne().HasForeignKey(e => e.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<IMApplicationRole>()
            .HasMany(r => r.Claims).WithOne()
            .HasForeignKey(r => r.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
