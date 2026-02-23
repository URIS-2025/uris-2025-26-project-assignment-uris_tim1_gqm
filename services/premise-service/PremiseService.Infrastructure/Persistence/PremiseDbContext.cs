using Microsoft.EntityFrameworkCore;
using PremiseService.Application.Interfaces;
using PremiseService.Domain.Entities;

namespace PremiseService.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for the Premise aggregate.
/// Implements IPremiseDbContext so Application services can query
/// the database without depending on Infrastructure.
/// </summary>
public class PremiseDbContext : DbContext, IPremiseDbContext
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
