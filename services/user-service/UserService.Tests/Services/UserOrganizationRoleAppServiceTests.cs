using AutoMapper;
using UserService.Application.DTOs;
using UserService.Application.Services;
using UserService.Application.Mappings;
using UserService.Application.Validators;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Tests.Helpers;
using FluentAssertions;
using FluentValidation;

namespace UserService.Tests.Services;

public class UserOrganizationRoleAppServiceTests : IDisposable
{
    private readonly Infrastructure.Persistence.UserServiceDbContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<AssignRoleRequest> _validator;
    private readonly UserOrganizationRoleAppService _sut;

    public UserOrganizationRoleAppServiceTests()
    {
        _context = TestDbContextFactory.Create();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<UserOrganizationRoleProfile>();
        });
        _mapper = config.CreateMapper();

        _validator = new AssignRoleRequestValidator();

        _sut = new UserOrganizationRoleAppService(_context, _mapper, _validator);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private async Task<User> SeedUser()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = $"test-{Guid.NewGuid():N}@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@1"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    private async Task<Role> SeedRole(string name = "Test Role")
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = "Desc",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
        return role;
    }

    [Fact]
    public async Task AssignRoleAsync_AssignsAndReturnsResponse()
    {
        var user = await SeedUser();
        var role = await SeedRole();
        var orgId = Guid.NewGuid();

        var request = new AssignRoleRequest
        {
            UserId = user.Id,
            RoleId = role.Id,
            OrganizationId = orgId
        };

        var result = await _sut.AssignRoleAsync(request);

        result.UserId.Should().Be(user.Id);
        result.RoleId.Should().Be(role.Id);
        result.RoleName.Should().Be("Test Role");
        _context.UserOrganizationRoles.Should().HaveCount(1);
    }

    [Fact]
    public async Task AssignRoleAsync_AllowsSameUserDifferentRolesSameOrg()
    {
        var user = await SeedUser();
        var role1 = await SeedRole("Role One");
        var role2 = await SeedRole("Role Two");
        var orgId = Guid.NewGuid();

        await _sut.AssignRoleAsync(new AssignRoleRequest 
        { 
            UserId = user.Id, RoleId = role1.Id, OrganizationId = orgId 
        });
        await _sut.AssignRoleAsync(new AssignRoleRequest 
        { 
            UserId = user.Id, RoleId = role2.Id, OrganizationId = orgId 
        });

        _context.UserOrganizationRoles.Should().HaveCount(2);
    }

    [Fact]
    public async Task AssignRoleAsync_ThrowsNotFoundException_WhenUserNotFound()
    {
        var role = await SeedRole();
        var request = new AssignRoleRequest
        {
            UserId = Guid.NewGuid(),
            RoleId = role.Id,
            OrganizationId = Guid.NewGuid()
        };

        var act = () => _sut.AssignRoleAsync(request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AssignRoleAsync_ThrowsNotFoundException_WhenRoleNotFound()
    {
        var user = await SeedUser();
        var request = new AssignRoleRequest
        {
            UserId = user.Id,
            RoleId = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid()
        };

        var act = () => _sut.AssignRoleAsync(request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AssignRoleAsync_ThrowsBadRequestException_WhenAlreadyAssigned()
    {
        var user = await SeedUser();
        var role = await SeedRole();
        var orgId = Guid.NewGuid();

        var request = new AssignRoleRequest
        {
            UserId = user.Id,
            RoleId = role.Id,
            OrganizationId = orgId
        };

        await _sut.AssignRoleAsync(request);
        var act = () => _sut.AssignRoleAsync(request);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task RemoveRoleAsync_RemovesAssignment()
    {
        var user = await SeedUser();
        var role = await SeedRole();
        var orgId = Guid.NewGuid();

        _context.UserOrganizationRoles.Add(new UserOrganizationRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            OrganizationId = orgId
        });
        await _context.SaveChangesAsync();

        await _sut.RemoveRoleAsync(user.Id, role.Id, orgId);

        _context.UserOrganizationRoles.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveRoleAsync_ThrowsNotFoundException_WhenNotFound()
    {
        var act = () => _sut.RemoveRoleAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsAssignments()
    {
        var user = await SeedUser();
        var role = await SeedRole();

        _context.UserOrganizationRoles.Add(new UserOrganizationRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            OrganizationId = Guid.NewGuid()
        });
        await _context.SaveChangesAsync();

        var result = await _sut.GetByUserIdAsync(user.Id);

        result.Should().HaveCount(1);
        result[0].RoleName.Should().Be("Test Role");
    }

    [Fact]
    public async Task GetByUserIdAsync_ThrowsNotFoundException_WhenUserNotFound()
    {
        var act = () => _sut.GetByUserIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
