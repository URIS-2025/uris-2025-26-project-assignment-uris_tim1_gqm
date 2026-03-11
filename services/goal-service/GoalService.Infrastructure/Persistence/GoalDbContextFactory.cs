using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GoalService.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core tooling (migrations).
/// Uses a localhost connection string for creating migrations locally.
/// </summary>
public class GoalDbContextFactory : IDesignTimeDbContextFactory<GoalDbContext>
{
    public GoalDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GoalDbContext>();
        
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

        optionsBuilder.UseNpgsql(connectionString);

        return new GoalDbContext(optionsBuilder.Options);
    }
}
