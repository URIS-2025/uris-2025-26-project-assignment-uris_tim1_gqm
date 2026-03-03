using UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Interfaces;

namespace UserService.Infrastructure.Persistence;

public class UserServiceDbContext : DbContext, IApplicationDbContext
{
    public UserServiceDbContext(DbContextOptions<UserServiceDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserOrganizationRole> UserOrganizationRoles => Set<UserOrganizationRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserServiceDbContext).Assembly);
    }
}
