using DepartmentService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DepartmentService.Tests.Helpers;

public static class TestDbContextFactory
{
    public static DepartmentServiceDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<DepartmentServiceDbContext>()
            .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
            .Options;

        var context = new DepartmentServiceDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
