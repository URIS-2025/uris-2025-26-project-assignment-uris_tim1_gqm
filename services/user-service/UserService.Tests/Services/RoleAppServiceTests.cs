using AutoMapper;
using UserService.Application.DTOs;
using UserService.Application.Services;
using UserService.Application.Mappings;
using UserService.Application.Validators;
using UserService.Domain.Constants;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Tests.Helpers;
using FluentAssertions;
using FluentValidation;

namespace UserService.Tests.Services;

public class RoleAppServiceTests : IDisposable
{
    private readonly Infrastructure.Persistence.UserServiceDbContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<RoleRequest> _validator;
    private readonly RoleAppService _sut;

    public RoleAppServiceTests()
    {
        _context = TestDbContextFactory.Create();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<RoleProfile>();
        });
        _mapper = config.CreateMapper();

        _validator = new RoleRequestValidator();

        _sut = new RoleAppService(_context, _mapper, _validator);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private async Task<Role> SeedRole(string name = "Test Role")
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = "A test role",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
        return role;
    }

    // ── GetAllAsync ──

    [Fact]
    public async Task GetAllAsync_ReturnsAllRoles()
    {
        await SeedRole("Alpha");
        await SeedRole("Beta");

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
        result.Select(r => r.Name).Should().BeInAscendingOrder();
    }

    // ── GetByIdAsync ──

    [Fact]
    public async Task GetByIdAsync_ReturnsRole_WhenExists()
    {
        var role = await SeedRole();

        var result = await _sut.GetByIdAsync(role.Id);

        result.Id.Should().Be(role.Id);
        result.Name.Should().Be("Test Role");
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFoundException_WhenNotFound()
    {
        var act = () => _sut.GetByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── CreateAsync ──

    [Fact]
    public async Task CreateAsync_CreatesAndReturnsRole()
    {
        var request = new RoleRequest { Name = "New Role", Description = "Desc" };

        var result = await _sut.CreateAsync(request);

        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("New Role");
        _context.Roles.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateAsync_ThrowsBadRequestException_WhenNameExists()
    {
        await SeedRole("Existing");
        var request = new RoleRequest { Name = "Existing", Description = "Desc" };

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task CreateAsync_ThrowsValidationException_WhenNameEmpty()
    {
        var request = new RoleRequest { Name = "" };

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    // ── UpdateAsync ──

    [Fact]
    public async Task UpdateAsync_UpdatesAndReturnsRole()
    {
        var role = await SeedRole("Old Name");
        var request = new RoleRequest { Name = "New Name", Description = "Updated" };

        var result = await _sut.UpdateAsync(role.Id, request);

        result.Name.Should().Be("New Name");
        result.Description.Should().Be("Updated");
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFoundException_WhenNotFound()
    {
        var request = new RoleRequest { Name = "Name" };

        var act = () => _sut.UpdateAsync(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_ThrowsBadRequestException_WhenNameTakenByOther()
    {
        await SeedRole("Taken");
        var role = await SeedRole("Original");
        var request = new RoleRequest { Name = "Taken" };

        var act = () => _sut.UpdateAsync(role.Id, request);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    // ── DeleteAsync ──

    [Fact]
    public async Task DeleteAsync_RemovesRole()
    {
        var role = await SeedRole("Custom Role");

        await _sut.DeleteAsync(role.Id);

        var deleted = await _context.Roles.FindAsync(role.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFoundException_WhenNotFound()
    {
        var act = () => _sut.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_ThrowsBadRequestException_WhenSystemRole()
    {
        var role = await SeedRole(Roles.SystemAdmin);

        var act = () => _sut.DeleteAsync(role.Id);

        await act.Should().ThrowAsync<BadRequestException>();
    }
}
