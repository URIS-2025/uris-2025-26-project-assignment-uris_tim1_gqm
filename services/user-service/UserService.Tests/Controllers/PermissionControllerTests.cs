using UserService.API.Controllers;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UserService.Tests.Controllers;

public class PermissionControllerTests
{
    private readonly Mock<IPermissionService> _serviceMock;
    private readonly PermissionController _sut;

    public PermissionControllerTests()
    {
        _serviceMock = new Mock<IPermissionService>();
        _sut = new PermissionController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithPermissions()
    {
        _serviceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync([new PermissionResponse { Name = "manage_users" }]);

        var result = await _sut.GetAll();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value.Should().BeOfType<List<PermissionResponse>>().Subject;
        value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetById_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(new PermissionResponse { Id = id });

        var result = await _sut.GetById(id);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var request = new PermissionRequest { Name = "new_perm", Description = "Desc" };
        var response = new PermissionResponse { Id = Guid.NewGuid(), Name = "new_perm" };
        _serviceMock.Setup(s => s.CreateAsync(request)).ReturnsAsync(response);

        var result = await _sut.Create(request);

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Update_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var request = new PermissionRequest { Name = "updated" };
        _serviceMock.Setup(s => s.UpdateAsync(id, request)).ReturnsAsync(new PermissionResponse { Name = "updated" });

        var result = await _sut.Update(id, request);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.DeleteAsync(id)).Returns(Task.CompletedTask);

        var result = await _sut.Delete(id);

        result.Should().BeOfType<NoContentResult>();
    }
}
