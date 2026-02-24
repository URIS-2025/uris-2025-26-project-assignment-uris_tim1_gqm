using AutoMapper;
using DepartmentService.Application.DTOs;
using DepartmentService.Application.Services;
using DepartmentService.Application.Mappings;
using DepartmentService.Application.Validators;
using DepartmentService.Domain.Entities;
using DepartmentService.Domain.Exceptions;
using DepartmentService.Tests.Helpers;
using FluentAssertions;
using FluentValidation;

namespace DepartmentService.Tests.Services;

public class DepartmentAppServiceTests : IDisposable
{
    private readonly Infrastructure.Persistence.DepartmentServiceDbContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<DepartmentRequest> _validator;
    private readonly DepartmentAppService _sut;

    public DepartmentAppServiceTests()
    {
        _context = TestDbContextFactory.Create();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<DepartmentProfile>();
        });
        _mapper = config.CreateMapper();

        _validator = new DepartmentRequestValidator();

        _sut = new DepartmentAppService(_context, _mapper, _validator);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private async Task<Organization> SeedOrganization(string name = "Test Org")
    {
        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Organizations.Add(org);
        await _context.SaveChangesAsync();
        return org;
    }

    private async Task<Department> SeedDepartment(Guid orgId, string name = "Test Dept")
    {
        var dept = new Department
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = "A test department",
            OrganizationId = orgId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Departments.Add(dept);
        await _context.SaveChangesAsync();
        return dept;
    }

    // ── GetAllAsync ──

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyPage_WhenNoDepartments()
    {
        var result = await _sut.GetAllAsync(1, 20);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResults()
    {
        var org = await SeedOrganization();
        await SeedDepartment(org.Id, "Alpha Dept");
        await SeedDepartment(org.Id, "Beta Dept");
        await SeedDepartment(org.Id, "Gamma Dept");

        var result = await _sut.GetAllAsync(1, 2);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsItemsOrderedByName()
    {
        var org = await SeedOrganization();
        await SeedDepartment(org.Id, "Zeta");
        await SeedDepartment(org.Id, "Alpha");

        var result = await _sut.GetAllAsync(1, 10);

        result.Items.Select(x => x.Name).Should().BeInAscendingOrder();
    }

    // ── GetByOrganizationIdAsync ──

    [Fact]
    public async Task GetByOrganizationIdAsync_ReturnsDepartments_ForGivenOrg()
    {
        var org1 = await SeedOrganization("Org 1");
        var org2 = await SeedOrganization("Org 2");
        await SeedDepartment(org1.Id, "Dept A");
        await SeedDepartment(org1.Id, "Dept B");
        await SeedDepartment(org2.Id, "Dept C");

        var result = await _sut.GetByOrganizationIdAsync(org1.Id, 1, 20);

        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(d => d.OrganizationId.Should().Be(org1.Id));
    }

    [Fact]
    public async Task GetByOrganizationIdAsync_ThrowsNotFoundException_WhenOrgNotFound()
    {
        var act = () => _sut.GetByOrganizationIdAsync(Guid.NewGuid(), 1, 20);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetByOrganizationIdAsync_ReturnsEmptyPage_WhenOrgHasNoDepts()
    {
        var org = await SeedOrganization();

        var result = await _sut.GetByOrganizationIdAsync(org.Id, 1, 20);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    // ── GetByIdAsync ──

    [Fact]
    public async Task GetByIdAsync_ReturnsDepartment_WhenExists()
    {
        var org = await SeedOrganization();
        var dept = await SeedDepartment(org.Id, "My Dept");

        var result = await _sut.GetByIdAsync(dept.Id);

        result.Id.Should().Be(dept.Id);
        result.Name.Should().Be("My Dept");
        result.OrganizationId.Should().Be(org.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFoundException_WhenNotFound()
    {
        var act = () => _sut.GetByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── CreateAsync ──

    [Fact]
    public async Task CreateAsync_CreatesAndReturnsDepartment()
    {
        var org = await SeedOrganization();
        var request = new DepartmentRequest
        {
            Name = "New Dept",
            Description = "Desc",
            OrganizationId = org.Id
        };

        var result = await _sut.CreateAsync(request);

        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("New Dept");
        result.OrganizationId.Should().Be(org.Id);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _context.Departments.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateAsync_ThrowsNotFoundException_WhenOrganizationNotFound()
    {
        var request = new DepartmentRequest
        {
            Name = "Dept",
            OrganizationId = Guid.NewGuid()
        };

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_ThrowsValidationException_WhenNameEmpty()
    {
        var org = await SeedOrganization();
        var request = new DepartmentRequest { Name = "", OrganizationId = org.Id };

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_ThrowsValidationException_WhenNameTooLong()
    {
        var org = await SeedOrganization();
        var request = new DepartmentRequest { Name = new string('A', 201), OrganizationId = org.Id };

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_ThrowsValidationException_WhenOrganizationIdEmpty()
    {
        var request = new DepartmentRequest { Name = "Valid", OrganizationId = Guid.Empty };

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    // ── UpdateAsync ──

    [Fact]
    public async Task UpdateAsync_UpdatesAndReturnsDepartment()
    {
        var org = await SeedOrganization();
        var dept = await SeedDepartment(org.Id, "Old Name");
        var request = new DepartmentRequest
        {
            Name = "New Name",
            Description = "Updated",
            OrganizationId = org.Id
        };

        var result = await _sut.UpdateAsync(dept.Id, request);

        result.Name.Should().Be("New Name");
        result.Description.Should().Be("Updated");
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFoundException_WhenDeptNotFound()
    {
        var org = await SeedOrganization();
        var request = new DepartmentRequest { Name = "Name", OrganizationId = org.Id };

        var act = () => _sut.UpdateAsync(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFoundException_WhenNewOrgNotFound()
    {
        var org = await SeedOrganization();
        var dept = await SeedDepartment(org.Id);
        var request = new DepartmentRequest { Name = "Name", OrganizationId = Guid.NewGuid() };

        var act = () => _sut.UpdateAsync(dept.Id, request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_ThrowsValidationException_WhenNameEmpty()
    {
        var org = await SeedOrganization();
        var dept = await SeedDepartment(org.Id);
        var request = new DepartmentRequest { Name = "", OrganizationId = org.Id };

        var act = () => _sut.UpdateAsync(dept.Id, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    // ── DeleteAsync ──

    [Fact]
    public async Task DeleteAsync_RemovesDepartment()
    {
        var org = await SeedOrganization();
        var dept = await SeedDepartment(org.Id);

        await _sut.DeleteAsync(dept.Id);

        _context.Departments.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFoundException_WhenNotFound()
    {
        var act = () => _sut.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
