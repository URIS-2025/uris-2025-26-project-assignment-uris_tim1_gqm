using AuditService.Application.Interfaces;
using AuditService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditService.Infrastructure.Data;

public class AuditDbContext : DbContext, IApplicationDbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ActorRole).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Service).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Metadata).HasColumnType("text");

            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.ActorId);
            entity.HasIndex(e => new { e.EntityId, e.EntityType });
        });
    }
}
