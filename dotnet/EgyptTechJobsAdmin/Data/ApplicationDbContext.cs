using Microsoft.EntityFrameworkCore;
using EgyptTechJobsAdmin.Models.Entities;

namespace EgyptTechJobsAdmin.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Job indexes
        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasIndex(e => e.Company);
            entity.HasIndex(e => e.Country);
            entity.HasIndex(e => e.WorkType);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.JobId).IsUnique();
        });

        // AdminUser indexes
        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // AuditLog indexes
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(e => e.EntityType);
            entity.HasIndex(e => e.PerformedAt);
        });
    }
}
