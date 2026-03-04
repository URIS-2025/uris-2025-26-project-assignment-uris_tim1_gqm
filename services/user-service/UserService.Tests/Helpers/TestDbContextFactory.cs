using UserService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace UserService.Tests.Helpers;

public static class TestDbContextFactory
{
    public static UserServiceDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<UserServiceDbContext>()
            .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
            .Options;

        var context = new UserServiceDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
