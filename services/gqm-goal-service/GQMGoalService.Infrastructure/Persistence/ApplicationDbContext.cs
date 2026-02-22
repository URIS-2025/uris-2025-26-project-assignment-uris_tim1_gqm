using Microsoft.EntityFrameworkCore;
using GQMGoalService.Domain.Entities;
using System.Reflection;

namespace GQMGoalService.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<GqmGoal> GqmGoals => Set<GqmGoal>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Target> Targets => Set<Target>();
    public DbSet<Measurement> Measurements => Set<Measurement>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }
}
