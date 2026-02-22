using GoalService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using GoalService.Application.Interfaces.Persistence;

namespace GoalService.Infrastructure.Persistence;

public class GoalDbContext : DbContext, IGoalDbContext
{
    public GoalDbContext(DbContextOptions<GoalDbContext> options) : base(options) { }

    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<Strategy> Strategies => Set<Strategy>();
    public DbSet<GoalInfluence> GoalInfluences => Set<GoalInfluence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GoalDbContext).Assembly);
    }
}
