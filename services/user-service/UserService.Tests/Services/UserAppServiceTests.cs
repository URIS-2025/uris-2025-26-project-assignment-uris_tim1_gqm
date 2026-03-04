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

public class UserAppServiceTests : IDisposable
{
    private readonly Infrastructure.Persistence.UserServiceDbContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<UserRequest> _validator;
    private readonly IValidator<UpdateProfileRequest> _profileValidator;
    private readonly UserAppService _sut;

    public UserAppServiceTests()
    {
        _context = TestDbContextFactory.Create();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<UserProfile>();
        });
        _mapper = config.CreateMapper();

        _validator = new UserRequestValidator();
        _profileValidator = new UpdateProfileRequestValidator();

        _sut = new UserAppService(_context, _mapper, _validator, _profileValidator);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private async Task<User> SeedUser(string email = "test@example.com", string firstName = "Test", string lastName = "User")
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@1"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    // ── GetAllAsync ──

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyPage_WhenNoUsers()
    {
        var result = await _sut.GetAllAsync(1, 10);

        result.Items.Should().BeEmpty();
        result.Total.Should().Be(0);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPagedResults()
    {
        await SeedUser("a@test.com", "Alice", "Smith");
        await SeedUser("b@test.com", "Bob", "Jones");
        await SeedUser("c@test.com", "Charlie", "Brown");

        var result = await _sut.GetAllAsync(1, 2);

        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(3);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsItemsOrderedByLastName()
    {
        await SeedUser("z@test.com", "Zoe", "Zeta");
        await SeedUser("a@test.com", "Alice", "Alpha");

        var result = await _sut.GetAllAsync(1, 10);

        result.Items.Select(x => x.LastName).Should().BeInAscendingOrder();
    }

    // ── GetByIdAsync ──

    [Fact]
    public async Task GetByIdAsync_ReturnsUser_WhenExists()
    {
        var user = await SeedUser();

        var result = await _sut.GetByIdAsync(user.Id);

        result.Id.Should().Be(user.Id);
        result.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFoundException_WhenNotFound()
    {
        var act = () => _sut.GetByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── GetByEmailAsync ──

    [Fact]
    public async Task GetByEmailAsync_ReturnsUser_WhenExists()
    {
        var user = await SeedUser();

        var result = await _sut.GetByEmailAsync("test@example.com");

        result.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_ThrowsNotFoundException_WhenNotFound()
    {
        var act = () => _sut.GetByEmailAsync("nonexistent@test.com");

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── CreateAsync ──

    [Fact]
    public async Task CreateAsync_CreatesAndReturnsUser()
    {
        var request = new UserRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            Password = "Password@1"
        };

        var result = await _sut.CreateAsync(request);

        result.Id.Should().NotBeEmpty();
        result.FirstName.Should().Be("John");
        result.Email.Should().Be("john@test.com");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _context.Users.Should().HaveCount(1);

        var dbUser = await _context.Users.FindAsync(result.Id);
        dbUser!.PasswordHash.Should().NotBe("Password@1");
        BCrypt.Net.BCrypt.Verify("Password@1", dbUser.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_ThrowsBadRequestException_WhenEmailExists()
    {
        await SeedUser("existing@test.com");
        var request = new UserRequest
        {
            FirstName = "New",
            LastName = "User",
            Email = "existing@test.com",
            Password = "Password@1"
        };

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task CreateAsync_ThrowsValidationException_WhenNameEmpty()
    {
        var request = new UserRequest { FirstName = "", LastName = "Doe", Email = "a@b.com", Password = "Password@1" };

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_ThrowsValidationException_WhenPasswordTooShort()
    {
        var request = new UserRequest { FirstName = "A", LastName = "B", Email = "a@b.com", Password = "Sh@1" };

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }

    // ── UpdateProfileAsync ──

    [Fact]
    public async Task UpdateProfileAsync_UpdatesAndReturnsUser()
    {
        var user = await SeedUser();
        var request = new UpdateProfileRequest { FirstName = "Updated", LastName = "Name" };

        var result = await _sut.UpdateProfileAsync(user.Id, request);

        result.FirstName.Should().Be("Updated");
        result.LastName.Should().Be("Name");
    }

    [Fact]
    public async Task UpdateProfileAsync_ThrowsNotFoundException_WhenNotFound()
    {
        var request = new UpdateProfileRequest { FirstName = "A", LastName = "B" };

        var act = () => _sut.UpdateProfileAsync(Guid.NewGuid(), request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── ToggleIsActiveAsync ──

    [Fact]
    public async Task ToggleIsActiveAsync_TogglesAndReturnsUser()
    {
        var user = await SeedUser();

        var result = await _sut.ToggleIsActiveAsync(user.Id);

        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleIsActiveAsync_ThrowsNotFoundException_WhenNotFound()
    {
        var act = () => _sut.ToggleIsActiveAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ── DeleteAsync ──

    [Fact]
    public async Task DeleteAsync_RemovesUser()
    {
        var user = await SeedUser();

        await _sut.DeleteAsync(user.Id);

        var deleted = await _context.Users.FindAsync(user.Id);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFoundException_WhenNotFound()
    {
        var act = () => _sut.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
