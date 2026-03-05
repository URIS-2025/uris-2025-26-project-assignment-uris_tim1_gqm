using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrchestrationService.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core tooling (migrations).
/// </summary>
public class OrchestrationDbContextFactory : IDesignTimeDbContextFactory<OrchestrationDbContext>
{
    public OrchestrationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrchestrationDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
        optionsBuilder.UseNpgsql(connectionString);

        return new OrchestrationDbContext(optionsBuilder.Options);
    }
}
