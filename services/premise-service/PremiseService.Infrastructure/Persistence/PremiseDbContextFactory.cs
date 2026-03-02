using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PremiseService.Infrastructure.Persistence;

public class PremiseDbContextFactory : IDesignTimeDbContextFactory<PremiseDbContext>
{
    public PremiseDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PremiseDbContext>();

        // Default connection string for design-time operations (migrations)
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
        optionsBuilder.UseNpgsql(connectionString);

        return new PremiseDbContext(optionsBuilder.Options);
    }
}
