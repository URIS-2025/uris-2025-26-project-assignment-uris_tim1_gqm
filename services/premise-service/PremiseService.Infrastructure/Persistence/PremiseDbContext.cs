using Microsoft.EntityFrameworkCore;
using PremiseService.Domain.Entities;

namespace PremiseService.Infrastructure.Persistence;

public class PremiseDbContext : DbContext
{
    public PremiseDbContext(DbContextOptions<PremiseDbContext> options)
        : base(options)
    {
    }

    public DbSet<Premise> Premises { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PremiseDbContext).Assembly);
    }
}
