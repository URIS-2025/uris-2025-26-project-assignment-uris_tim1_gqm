using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AssessmentService.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core tooling (migrations).
/// Uses DATABASE_URL environment variable.
/// </summary>
public class AssessmentDbContextFactory : IDesignTimeDbContextFactory<AssessmentDbContext>
{
    public AssessmentDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AssessmentDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("DATABASE_URL environment variable is not set.");

        optionsBuilder.UseNpgsql(connectionString);

        return new AssessmentDbContext(optionsBuilder.Options);
    }
}