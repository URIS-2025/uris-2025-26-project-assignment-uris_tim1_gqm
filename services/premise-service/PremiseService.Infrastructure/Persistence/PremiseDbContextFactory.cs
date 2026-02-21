using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PremiseService.Infrastructure.Persistence;

public class PremiseDbContextFactory : IDesignTimeDbContextFactory<PremiseDbContext>
{
    public PremiseDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PremiseDbContext>();

        // Default connection string for design-time operations (migrations)
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=premisedb;Username=postgres;Password=postgres");

        return new PremiseDbContext(optionsBuilder.Options);
    }
}
