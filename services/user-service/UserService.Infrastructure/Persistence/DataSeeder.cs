using UserService.Domain.Constants;
using UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace UserService.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(UserServiceDbContext context, ILogger logger)
    {
        if (await context.Roles.AnyAsync())
        {
            logger.LogInformation("Database already seeded. Skipping seed data.");
            return;
        }

        logger.LogInformation("Seeding database with initial data...");

        // ── Roles ──
        var systemAdmin = new Role
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
            Name = Roles.SystemAdmin,
            Description = "Full system access. Can manage all organizations, users, roles, and permissions.",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var orgAdmin = new Role
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
            Name = Roles.OrganizationAdmin,
            Description = "Manages users, roles, and departments within their organization.",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var deptManager = new Role
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000003"),
            Name = Roles.DepartmentManager,
            Description = "Manages goals and measurements within their department.",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var analyst = new Role
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000004"),
            Name = Roles.Analyst,
            Description = "Can view goals, record measurements, and manage probability assessments.",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var viewer = new Role
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000005"),
            Name = Roles.Viewer,
            Description = "Read-only access to goals and analytics.",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Roles.AddRange(systemAdmin, orgAdmin, deptManager, analyst, viewer);

        // ── Permissions ──
        var manageOrganizations = CreatePermission("20000000-0000-0000-0000-000000000001", Permissions.ManageOrganizations, "Create, update, and delete organizations.");
        var manageUsers = CreatePermission("20000000-0000-0000-0000-000000000002", Permissions.ManageUsers, "Create, update, and delete users.");
        var manageRoles = CreatePermission("20000000-0000-0000-0000-000000000003", Permissions.ManageRoles, "Create, update, and delete roles.");
        var manageDepartments = CreatePermission("20000000-0000-0000-0000-000000000004", Permissions.ManageDepartments, "Create, update, and delete departments.");
        var viewAllDepartments = CreatePermission("20000000-0000-0000-0000-000000000005", Permissions.ViewAllDepartments, "View all departments across the organization.");
        var createGoals = CreatePermission("20000000-0000-0000-0000-000000000006", Permissions.CreateGoals, "Create new goals.");
        var editGoals = CreatePermission("20000000-0000-0000-0000-000000000007", Permissions.EditGoals, "Edit existing goals.");
        var deleteGoals = CreatePermission("20000000-0000-0000-0000-000000000008", Permissions.DeleteGoals, "Delete goals.");
        var viewGoals = CreatePermission("20000000-0000-0000-0000-000000000009", Permissions.ViewGoals, "View goals.");
        var manageGoalInfluences = CreatePermission("20000000-0000-0000-0000-0000000000010", Permissions.ManageGoalInfluences, "Manage goal influences and relationships.");
        var recordMeasurements = CreatePermission("20000000-0000-0000-0000-0000000000011", Permissions.RecordMeasurements, "Record measurement values.");
        var manageProbabilityAssessments = CreatePermission("20000000-0000-0000-0000-0000000000012", Permissions.ManageProbabilityAssessments, "Create and manage probability assessments.");
        var viewAnalytics = CreatePermission("20000000-0000-0000-0000-0000000000013", Permissions.ViewAnalytics, "View analytics and reports.");

        context.Permissions.AddRange(
            manageOrganizations, manageUsers, manageRoles, manageDepartments,
            viewAllDepartments, createGoals, editGoals, deleteGoals,
            viewGoals, manageGoalInfluences, recordMeasurements,
            manageProbabilityAssessments, viewAnalytics);

        // ── Role-Permission Mappings ──
        var rolePermissions = new List<RolePermission>
        {
            // System Admin — all 13 permissions
            Map(systemAdmin, manageOrganizations),
            Map(systemAdmin, manageUsers),
            Map(systemAdmin, manageRoles),
            Map(systemAdmin, manageDepartments),
            Map(systemAdmin, viewAllDepartments),
            Map(systemAdmin, createGoals),
            Map(systemAdmin, editGoals),
            Map(systemAdmin, deleteGoals),
            Map(systemAdmin, viewGoals),
            Map(systemAdmin, manageGoalInfluences),
            Map(systemAdmin, recordMeasurements),
            Map(systemAdmin, manageProbabilityAssessments),
            Map(systemAdmin, viewAnalytics),

            // Organization Admin
            Map(orgAdmin, manageUsers),
            Map(orgAdmin, manageRoles),
            Map(orgAdmin, manageDepartments),
            Map(orgAdmin, viewAllDepartments),
            Map(orgAdmin, createGoals),
            Map(orgAdmin, editGoals),
            Map(orgAdmin, deleteGoals),
            Map(orgAdmin, viewGoals),
            Map(orgAdmin, manageGoalInfluences),
            Map(orgAdmin, recordMeasurements),
            Map(orgAdmin, manageProbabilityAssessments),
            Map(orgAdmin, viewAnalytics),

            // Department Manager
            Map(deptManager, createGoals),
            Map(deptManager, editGoals),
            Map(deptManager, viewGoals),
            Map(deptManager, manageGoalInfluences),
            Map(deptManager, recordMeasurements),
            Map(deptManager, manageProbabilityAssessments),
            Map(deptManager, viewAnalytics),

            // Analyst
            Map(analyst, viewGoals),
            Map(analyst, recordMeasurements),
            Map(analyst, manageProbabilityAssessments),
            Map(analyst, viewAnalytics),

            // Viewer
            Map(viewer, viewGoals),
            Map(viewer, viewAnalytics),
        };

        context.RolePermissions.AddRange(rolePermissions);

        // ── Default System Admin User ──
        var adminUser = new User
        {
            Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
            FirstName = "System",
            LastName = "Admin",
            Email = "admin@gqmplus.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(adminUser);

        await context.SaveChangesAsync();

        logger.LogInformation(
            "Database seeded successfully with {RoleCount} roles, {PermissionCount} permissions, {MappingCount} role-permission mappings, and 1 admin user.",
            5, 13, rolePermissions.Count);
    }

    private static Permission CreatePermission(string id, string name, string description)
    {
        return new Permission
        {
            Id = Guid.Parse(id),
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static RolePermission Map(Role role, Permission permission)
    {
        return new RolePermission
        {
            RoleId = role.Id,
            PermissionId = permission.Id
        };
    }
}
