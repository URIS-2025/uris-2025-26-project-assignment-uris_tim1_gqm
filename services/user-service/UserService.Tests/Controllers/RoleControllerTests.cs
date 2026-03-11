using UserService.API.Controllers;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UserService.Tests.Controllers;

public class RoleControllerTests
{
    private readonly Mock<IRoleService> _serviceMock;
    private readonly RoleController _sut;

    public RoleControllerTests()
    {
        _serviceMock = new Mock<IRoleService>();
        _sut = new RoleController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithRoles()
    {
        _serviceMock.Setup(s => s.GetAllAsync())
            .ReturnsAsync([new RoleResponse { Name = "Admin" }]);

        var result = await _sut.GetAll();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value.Should().BeOfType<List<RoleResponse>>().Subject;
        value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetById_ReturnsOk()
    {
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(new RoleResponse { Id = id });

        var result = await _sut.GetById(id);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var request = new RoleRequest { Name = "New", Description = "Desc" };
        var response = new RoleResponse { Id = Guid.NewGuid(), Name = "New" };
        _serviceMock.Setup(s => s.CreateAsync(request)).ReturnsAsync(response);

        var result = await _sut.Create(request);

        result.Result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Update_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var request = new RoleRequest { Name = "Updated" };
        _serviceMock.Setup(s => s.UpdateAsync(id, request)).ReturnsAsync(new RoleResponse { Name = "Updated" });

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
