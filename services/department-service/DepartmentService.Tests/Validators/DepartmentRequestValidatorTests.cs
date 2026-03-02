using DepartmentService.Application.DTOs;
using DepartmentService.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace DepartmentService.Tests.Validators;

public class DepartmentRequestValidatorTests
{
    private readonly DepartmentRequestValidator _sut = new();

    [Fact]
    public void Should_Pass_WhenValidRequest()
    {
        var request = new DepartmentRequest
        {
            Name = "Valid Dept",
            Description = "Desc",
            OrganizationId = Guid.NewGuid()
        };

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_WhenNameEmpty()
    {
        var request = new DepartmentRequest { Name = "", OrganizationId = Guid.NewGuid() };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Fail_WhenNameExceeds200Characters()
    {
        var request = new DepartmentRequest
        {
            Name = new string('A', 201),
            OrganizationId = Guid.NewGuid()
        };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Pass_WhenNameIs200Characters()
    {
        var request = new DepartmentRequest
        {
            Name = new string('A', 200),
            OrganizationId = Guid.NewGuid()
        };

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Fail_WhenDescriptionExceeds1000Characters()
    {
        var request = new DepartmentRequest
        {
            Name = "Valid",
            Description = new string('A', 1001),
            OrganizationId = Guid.NewGuid()
        };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Pass_WhenDescriptionIsNull()
    {
        var request = new DepartmentRequest
        {
            Name = "Valid",
            Description = null,
            OrganizationId = Guid.NewGuid()
        };

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Fail_WhenOrganizationIdEmpty()
    {
        var request = new DepartmentRequest { Name = "Valid", OrganizationId = Guid.Empty };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.OrganizationId);
    }
}
