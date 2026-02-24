using DepartmentService.API.Controllers;
using DepartmentService.Application.DTOs;
using DepartmentService.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DepartmentService.Tests.Controllers;

public class DepartmentControllerTests
{
    private readonly Mock<IDepartmentService> _serviceMock;
    private readonly DepartmentController _sut;

    public DepartmentControllerTests()
    {
        _serviceMock = new Mock<IDepartmentService>();
        _sut = new DepartmentController(_serviceMock.Object);
    }

    // ── GetAll ──

    [Fact]
    public async Task GetAll_ReturnsOk_WithPagedResponse()
    {
        var pagedResponse = new PagedResponse<DepartmentResponse>
        {
            Items = [new DepartmentResponse { Id = Guid.NewGuid(), Name = "Dept 1" }],
            TotalCount = 1, Page = 1, Size = 20, TotalPages = 1
        };
        _serviceMock.Setup(s => s.GetAllAsync(1, 20)).ReturnsAsync(pagedResponse);

        var result = await _sut.GetAll();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value.Should().BeOfType<PagedResponse<DepartmentResponse>>().Subject;
        value.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAll_CallsServiceWithCorrectPagination()
    {
        _serviceMock.Setup(s => s.GetAllAsync(2, 5))
            .ReturnsAsync(new PagedResponse<DepartmentResponse>());

        await _sut.GetAll(page: 2, size: 5);

        _serviceMock.Verify(s => s.GetAllAsync(2, 5), Times.Once);
    }

    // ── GetById ──

    [Fact]
    public async Task GetById_ReturnsOk_WhenFound()
    {
        var id = Guid.NewGuid();
        var response = new DepartmentResponse { Id = id, Name = "Test Dept" };
        _serviceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(response);

        var result = await _sut.GetById(id);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value.Should().BeOfType<DepartmentResponse>().Subject;
        value.Id.Should().Be(id);
    }

    // ── GetByOrganizationId ──

    [Fact]
    public async Task GetByOrganizationId_ReturnsOk_WithPagedResponse()
    {
        var orgId = Guid.NewGuid();
        var pagedResponse = new PagedResponse<DepartmentResponse>
        {
            Items = [new DepartmentResponse { OrganizationId = orgId }],
            TotalCount = 1, Page = 1, Size = 20, TotalPages = 1
        };
        _serviceMock.Setup(s => s.GetByOrganizationIdAsync(orgId, 1, 20)).ReturnsAsync(pagedResponse);

        var result = await _sut.GetByOrganizationId(orgId);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value.Should().BeOfType<PagedResponse<DepartmentResponse>>().Subject;
        value.Items.Should().HaveCount(1);
    }

    // ── Create ──

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var request = new DepartmentRequest { Name = "New", OrganizationId = Guid.NewGuid() };
        var response = new DepartmentResponse { Id = Guid.NewGuid(), Name = "New" };
        _serviceMock.Setup(s => s.CreateAsync(request)).ReturnsAsync(response);

        var result = await _sut.Create(request);

        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(DepartmentController.GetById));
        var value = createdResult.Value.Should().BeOfType<DepartmentResponse>().Subject;
        value.Name.Should().Be("New");
    }

    // ── Update ──

    [Fact]
    public async Task Update_ReturnsOk_WithUpdatedDepartment()
    {
        var id = Guid.NewGuid();
        var request = new DepartmentRequest { Name = "Updated", OrganizationId = Guid.NewGuid() };
        var response = new DepartmentResponse { Id = id, Name = "Updated" };
        _serviceMock.Setup(s => s.UpdateAsync(id, request)).ReturnsAsync(response);

        var result = await _sut.Update(id, request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value.Should().BeOfType<DepartmentResponse>().Subject;
        value.Name.Should().Be("Updated");
    }

    // ── Delete ──

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
