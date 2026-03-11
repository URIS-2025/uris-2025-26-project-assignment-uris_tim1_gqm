using System.Security.Claims;
using UserService.API.Controllers;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Application.Interfaces.Clients;
using Shared.Contracts;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UserService.Tests.Controllers;

public class UserControllerTests
{
    private readonly Mock<IUserService> _serviceMock;
    private readonly Mock<IAuditClient> _auditClientMock;
    private readonly UserController _sut;

    public UserControllerTests()
    {
        _serviceMock = new Mock<IUserService>();
        _auditClientMock = new Mock<IAuditClient>();
        _auditClientMock.Setup(a => a.LogAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<object?>())).Returns(Task.CompletedTask);
        _sut = new UserController(_serviceMock.Object, _auditClientMock.Object);
        SetupUser(Guid.NewGuid());
    }

    private void SetupUser(Guid userId, bool isSystemAdmin = false, Guid? orgId = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        
        if (isSystemAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, UserService.Domain.Constants.Roles.SystemAdmin));
        }

        if (orgId.HasValue)
        {
            claims.Add(new Claim(Shared.Auth.ClaimsPrincipalExtensions.OrganizationIdClaimType, orgId.Value.ToString()));
        }

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithPagedResponse()
    {
        var userId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        SetupUser(userId, true, orgId);

        var pagedResponse = new PaginationResponse<UserResponse>
        {
            Items = [new UserResponse { Id = Guid.NewGuid(), FirstName = "Test" }],
            Total = 1, PageNumber = 1, PageSize = 10
        };
        _serviceMock.Setup(s => s.GetAllAsync(1, 10, userId, true, orgId)).ReturnsAsync(pagedResponse);

        var result = await _sut.GetAll(1, 10);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value.Should().BeOfType<PaginationResponse<UserResponse>>().Subject;
        value.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenFound()
    {
        var id = Guid.NewGuid();
        var response = new UserResponse { Id = id, FirstName = "John" };
        _serviceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(response);

        var result = await _sut.GetById(id);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value.Should().BeOfType<UserResponse>().Subject;
        value.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetByEmail_ReturnsOk_WhenFound()
    {
        var response = new UserResponse { Email = "test@test.com" };
        _serviceMock.Setup(s => s.GetByEmailAsync("test@test.com")).ReturnsAsync(response);

        var result = await _sut.GetByEmail("test@test.com");

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var request = new UserRequest { FirstName = "New", LastName = "User", Email = "new@test.com", Password = "Password@1" };
        var response = new UserResponse { Id = Guid.NewGuid(), FirstName = "New" };
        _serviceMock.Setup(s => s.CreateAsync(request)).ReturnsAsync(response);

        var result = await _sut.Create(request);

        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(UserController.GetById));
    }

    [Fact]
    public async Task UpdateProfile_ReturnsOk_WhenUserUpdatesOwnProfile()
    {
        var userId = Guid.NewGuid();
        var request = new UpdateProfileRequest { FirstName = "Updated", LastName = "Name" };
        var response = new UserResponse { Id = userId, FirstName = "Updated", LastName = "Name" };

        _serviceMock.Setup(s => s.UpdateProfileAsync(userId, request)).ReturnsAsync(response);

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "Test"));
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        var result = await _sut.UpdateProfile(userId, request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value.Should().BeOfType<UserResponse>().Subject;
        value.FirstName.Should().Be("Updated");
    }

    [Fact]
    public async Task UpdateProfile_ReturnsForbid_WhenUserUpdatesOtherProfile()
    {
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var request = new UpdateProfileRequest { FirstName = "Updated", LastName = "Name" };

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "Test"));
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        var result = await _sut.UpdateProfile(differentUserId, request);

        result.Result.Should().BeOfType<ForbidResult>();
        _serviceMock.Verify(s => s.UpdateProfileAsync(It.IsAny<Guid>(), It.IsAny<UpdateProfileRequest>()), Times.Never);
    }

    [Fact]
    public async Task ToggleIsActive_ReturnsOk()
    {
        var userId = Guid.NewGuid();
        var response = new UserResponse { Id = userId, IsActive = false };

        _serviceMock.Setup(s => s.ToggleIsActiveAsync(userId)).ReturnsAsync(response);

        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                }, "Test"))
            }
        };

        var result = await _sut.ToggleIsActive(userId);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value.Should().BeOfType<UserResponse>().Subject;
        value.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.DeleteAsync(id)).Returns(Task.CompletedTask);

        var result = await _sut.Delete(id);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.DeleteAsync(id), Times.Once);
    }
}
