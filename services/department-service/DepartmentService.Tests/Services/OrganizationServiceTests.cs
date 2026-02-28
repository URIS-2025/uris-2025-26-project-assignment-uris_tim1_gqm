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

public class OrganizationServiceTests : IDisposable
{
    private readonly Infrastructure.Persistence.DepartmentServiceDbContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<OrganizationRequest> _validator;
    private readonly OrganizationService _sut;

    public OrganizationServiceTests()
    {
        _context = TestDbContextFactory.Create();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<OrganizationProfile>();
        });
        _mapper = config.CreateMapper();

        _validator = new OrganizationRequestValidator();

        _sut = new OrganizationService(_context, _mapper, _validator);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private async Task<Organization> SeedOrganization(string name = "Test Org", string? description = "A test organization")
    {
        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Organizations.Add(org);
        await _context.SaveChangesAsync();
        return org;
    }

    // ── GetAllAsync ──

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyPage_WhenNoOrganizations()
    {
        var result = await _sut.GetAllAsync(1, 20);

        result.Items.Should().BeEmpty();
        result.Total.Should().Be(0);
        result.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResults()
    {
        await SeedOrganization("Alpha Corp");
        await SeedOrganization("Beta Inc");
        await SeedOrganization("Gamma Ltd");

        var result = await _sut.GetAllAsync(1, 2);

        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(3);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsSecondPage()
    {
        await SeedOrganization("Alpha Corp");
        await SeedOrganization("Beta Inc");
        await SeedOrganization("Gamma Ltd");

        var result = await _sut.GetAllAsync(2, 2);

        result.Items.Should().HaveCount(1);
        result.PageNumber.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsItemsOrderedByName()
    {
        await SeedOrganization("Zeta");
        await SeedOrganization("Alpha");
        await SeedOrganization("Mu");

        var result = await _sut.GetAllAsync(1, 10);

        result.Items.Select(x => x.Name).Should().BeInAscendingOrder();
    }

    // ── GetByIdAsync ──

    [Fact]
    public async Task GetByIdAsync_ReturnsOrganization_WhenExists()
    {
        var org = await SeedOrganization("Test Org");

        var result = await _sut.GetByIdAsync(org.Id);

        result.Id.Should().Be(org.Id);
        result.Name.Should().Be("Test Org");
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFoundException_WhenNotFound()
    {
        var act = () => _sut.GetByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── CreateAsync ──

    [Fact]
    public async Task CreateAsync_CreatesAndReturnsOrganization()
    {
        var request = new OrganizationRequest { Name = "New Org", Description = "Desc" };

        var result = await _sut.CreateAsync(request);

        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("New Org");
        result.Description.Should().Be("Desc");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _context.Organizations.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateAsync_ThrowsValidationException_WhenNameEmpty()
    {
        var request = new OrganizationRequest { Name = "", Description = "Desc" };

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_ThrowsValidationException_WhenNameTooLong()
    {
        var request = new OrganizationRequest { Name = new string('A', 201) };

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_ThrowsValidationException_WhenDescriptionTooLong()
    {
        var request = new OrganizationRequest { Name = "Valid", Description = new string('A', 1001) };

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    // ── UpdateAsync ──

    [Fact]
    public async Task UpdateAsync_UpdatesAndReturnsOrganization()
    {
        var org = await SeedOrganization("Old Name");
        var request = new OrganizationRequest { Name = "New Name", Description = "Updated" };

        var result = await _sut.UpdateAsync(org.Id, request);

        result.Name.Should().Be("New Name");
        result.Description.Should().Be("Updated");
        result.UpdatedAt.Should().BeOnOrAfter(org.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFoundException_WhenNotFound()
    {
        var request = new OrganizationRequest { Name = "Name" };

        var act = () => _sut.UpdateAsync(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_ThrowsValidationException_WhenNameEmpty()
    {
        var org = await SeedOrganization();
        var request = new OrganizationRequest { Name = "" };

        var act = () => _sut.UpdateAsync(org.Id, request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    // ── DeleteAsync ──

    [Fact]
    public async Task DeleteAsync_RemovesOrganization()
    {
        var org = await SeedOrganization();

        await _sut.DeleteAsync(org.Id);

        _context.Organizations.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFoundException_WhenNotFound()
    {
        var act = () => _sut.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
