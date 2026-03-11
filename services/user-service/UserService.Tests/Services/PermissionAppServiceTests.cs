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

public class PermissionAppServiceTests : IDisposable
{
    private readonly Infrastructure.Persistence.UserServiceDbContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<PermissionRequest> _validator;
    private readonly PermissionAppService _sut;

    public PermissionAppServiceTests()
    {
        _context = TestDbContextFactory.Create();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<PermissionProfile>();
        });
        _mapper = config.CreateMapper();

        _validator = new PermissionRequestValidator();

        _sut = new PermissionAppService(_context, _mapper, _validator);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private async Task<Permission> SeedPermission(string name = "test_permission")
    {
        var permission = new Permission
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = "A test permission",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync();
        return permission;
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllPermissions()
    {
        await SeedPermission("alpha");
        await SeedPermission("beta");

        var result = await _sut.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsPermission_WhenExists()
    {
        var permission = await SeedPermission();

        var result = await _sut.GetByIdAsync(permission.Id);

        result.Id.Should().Be(permission.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFoundException_WhenNotFound()
    {
        var act = () => _sut.GetByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_CreatesAndReturnsPermission()
    {
        var request = new PermissionRequest { Name = "new_perm", Description = "Desc" };

        var result = await _sut.CreateAsync(request);

        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("new_perm");
    }

    [Fact]
    public async Task CreateAsync_ThrowsBadRequestException_WhenNameExists()
    {
        await SeedPermission("existing");
        var request = new PermissionRequest { Name = "existing" };

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAndReturnsPermission()
    {
        var permission = await SeedPermission("old_name");
        var request = new PermissionRequest { Name = "new_name", Description = "Updated" };

        var result = await _sut.UpdateAsync(permission.Id, request);

        result.Name.Should().Be("new_name");
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFoundException_WhenNotFound()
    {
        var request = new PermissionRequest { Name = "name" };

        var act = () => _sut.UpdateAsync(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_RemovesPermission()
    {
        var permission = await SeedPermission("custom_perm");

        await _sut.DeleteAsync(permission.Id);

        var deleted = await _context.Permissions.FindAsync(permission.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFoundException_WhenNotFound()
    {
        var act = () => _sut.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_ThrowsBadRequestException_WhenSystemPermission()
    {
        var permission = await SeedPermission(Permissions.ManageUsers);

        var act = () => _sut.DeleteAsync(permission.Id);

        await act.Should().ThrowAsync<BadRequestException>();
    }
}
