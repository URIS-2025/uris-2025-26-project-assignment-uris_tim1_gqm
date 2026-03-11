using DepartmentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

using DepartmentService.Application.Interfaces;

namespace DepartmentService.Infrastructure.Persistence;

public class DepartmentServiceDbContext : DbContext, IApplicationDbContext
{
    public DepartmentServiceDbContext(DbContextOptions<DepartmentServiceDbContext> options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Department> Departments => Set<Department>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DepartmentServiceDbContext).Assembly);
    }
}
