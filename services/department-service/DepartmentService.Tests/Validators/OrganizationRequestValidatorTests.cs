using DepartmentService.Application.DTOs;
using DepartmentService.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace DepartmentService.Tests.Validators;

public class OrganizationRequestValidatorTests
{
    private readonly OrganizationRequestValidator _sut = new();

    [Fact]
    public void Should_Pass_WhenValidRequest()
    {
        var request = new OrganizationRequest { Name = "Valid Org", Description = "Desc" };

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_WhenNameEmpty()
    {
        var request = new OrganizationRequest { Name = "" };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Fail_WhenNameExceeds200Characters()
    {
        var request = new OrganizationRequest { Name = new string('A', 201) };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Pass_WhenNameIs200Characters()
    {
        var request = new OrganizationRequest { Name = new string('A', 200) };

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Fail_WhenDescriptionExceeds1000Characters()
    {
        var request = new OrganizationRequest { Name = "Valid", Description = new string('A', 1001) };

        var result = _sut.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Pass_WhenDescriptionIsNull()
    {
        var request = new OrganizationRequest { Name = "Valid", Description = null };

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }
}
