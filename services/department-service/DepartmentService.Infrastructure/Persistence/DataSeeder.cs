using DepartmentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DepartmentService.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(DepartmentServiceDbContext context, ILogger logger)
    {
        if (await context.Organizations.AnyAsync())
        {
            logger.LogInformation("Database already seeded. Skipping seed data.");
            return;
        }

        logger.LogInformation("Seeding database with initial data...");

        var orgTechCorp = new Organization
        {
            Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
            Name = "TechCorp International",
            Description = "A leading technology company specializing in enterprise software solutions.",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var orgGreenEnergy = new Organization
        {
            Id = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
            Name = "GreenEnergy Solutions",
            Description = "Renewable energy company focused on sustainable power generation.",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var orgHealthPlus = new Organization
        {
            Id = Guid.Parse("c3d4e5f6-a7b8-9012-cdef-123456789012"),
            Name = "HealthPlus Medical",
            Description = "Healthcare organization providing innovative medical services.",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Organizations.AddRange(orgTechCorp, orgGreenEnergy, orgHealthPlus);

        var departments = new List<Department>
        {
            new()
            {
                Id = Guid.Parse("d4e5f6a7-b8c9-0123-defa-234567890123"),
                Name = "Software Engineering",
                Description = "Responsible for developing and maintaining software products.",
                OrganizationId = orgTechCorp.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("e5f6a7b8-c9d0-1234-efab-345678901234"),
                Name = "Quality Assurance",
                Description = "Ensures product quality through testing and validation.",
                OrganizationId = orgTechCorp.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("f6a7b8c9-d0e1-2345-fabc-456789012345"),
                Name = "Human Resources",
                Description = "Manages employee relations and organizational culture.",
                OrganizationId = orgTechCorp.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("a7b8c9d0-e1f2-3456-abcd-567890123456"),
                Name = "Solar Division",
                Description = "Focuses on solar panel technology and installation.",
                OrganizationId = orgGreenEnergy.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("b8c9d0e1-f2a3-4567-bcde-678901234567"),
                Name = "Wind Energy",
                Description = "Develops and manages wind farm operations.",
                OrganizationId = orgGreenEnergy.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("c9d0e1f2-a3b4-5678-cdef-789012345678"),
                Name = "Cardiology",
                Description = "Specialized department for heart and cardiovascular care.",
                OrganizationId = orgHealthPlus.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.Parse("d0e1f2a3-b4c5-6789-defa-890123456789"),
                Name = "Radiology",
                Description = "Provides imaging and diagnostic services.",
                OrganizationId = orgHealthPlus.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        context.Departments.AddRange(departments);
        await context.SaveChangesAsync();

        logger.LogInformation("Database seeded successfully with {OrgCount} organizations and {DeptCount} departments.",
            3, departments.Count);
    }
}
