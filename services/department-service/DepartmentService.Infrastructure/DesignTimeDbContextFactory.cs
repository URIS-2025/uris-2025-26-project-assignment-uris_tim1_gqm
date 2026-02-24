using DepartmentService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DepartmentService.Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DepartmentServiceDbContext>
{
    public DepartmentServiceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DepartmentServiceDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=departmentdb;Username=postgres;Password=postgres");

        return new DepartmentServiceDbContext(optionsBuilder.Options);
    }
}
