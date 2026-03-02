using DepartmentService.API.Controllers;
using DepartmentService.Application.DTOs;
using Shared.Contracts;
using DepartmentService.Application.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DepartmentService.Tests.Controllers;

public class OrganizationControllerTests
{
    private readonly Mock<IOrganizationService> _serviceMock;
    private readonly OrganizationController _sut;

    public OrganizationControllerTests()
    {
        _serviceMock = new Mock<IOrganizationService>();
        _sut = new OrganizationController(_serviceMock.Object);
    }

    // ── GetAll ──

    [Fact]
    public async Task GetAll_ReturnsOk_WithPagedResponse()
    {
        var pagedResponse = new PaginationResponse<OrganizationResponse>
        {
            Items = [new OrganizationResponse { Id = Guid.NewGuid(), Name = "Org 1" }],
            Total = 1, PageNumber = 1, PageSize = 20
        };
        _serviceMock.Setup(s => s.GetAllAsync(1, 20)).ReturnsAsync(pagedResponse);

        var result = await _sut.GetAll();

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value.Should().BeOfType<PaginationResponse<OrganizationResponse>>().Subject;
        value.Items.Should().HaveCount(1);
        value.Total.Should().Be(1);
    }

    [Fact]
    public async Task GetAll_CallsServiceWithCorrectPagination()
    {
        _serviceMock.Setup(s => s.GetAllAsync(3, 10))
            .ReturnsAsync(new PaginationResponse<OrganizationResponse>());

        await _sut.GetAll(page: 3, size: 10);

        _serviceMock.Verify(s => s.GetAllAsync(3, 10), Times.Once);
    }

    // ── GetById ──

    [Fact]
    public async Task GetById_ReturnsOk_WhenFound()
    {
        var id = Guid.NewGuid();
        var response = new OrganizationResponse { Id = id, Name = "Test" };
        _serviceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(response);

        var result = await _sut.GetById(id);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value.Should().BeOfType<OrganizationResponse>().Subject;
        value.Id.Should().Be(id);
    }

    // ── Create ──

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var request = new OrganizationRequest { Name = "New Org" };
        var response = new OrganizationResponse { Id = Guid.NewGuid(), Name = "New Org" };
        _serviceMock.Setup(s => s.CreateAsync(request)).ReturnsAsync(response);

        var result = await _sut.Create(request);

        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(OrganizationController.GetById));
        var value = createdResult.Value.Should().BeOfType<OrganizationResponse>().Subject;
        value.Name.Should().Be("New Org");
    }

    // ── Update ──

    [Fact]
    public async Task Update_ReturnsOk_WithUpdatedOrganization()
    {
        var id = Guid.NewGuid();
        var request = new OrganizationRequest { Name = "Updated" };
        var response = new OrganizationResponse { Id = id, Name = "Updated" };
        _serviceMock.Setup(s => s.UpdateAsync(id, request)).ReturnsAsync(response);

        var result = await _sut.Update(id, request);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var value = okResult.Value.Should().BeOfType<OrganizationResponse>().Subject;
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
