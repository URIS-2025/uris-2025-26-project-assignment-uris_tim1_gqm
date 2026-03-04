using UserService.Application.DTOs;
using UserService.Application.Validators;
using FluentValidation.TestHelper;

namespace UserService.Tests.Validators;

public class RoleRequestValidatorTests
{
    private readonly RoleRequestValidator _sut = new();

    [Fact]
    public void Should_Pass_WhenValid()
    {
        var request = new RoleRequest { Name = "Admin", Description = "Desc" };
        _sut.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_WhenNameEmpty()
    {
        var request = new RoleRequest { Name = "" };
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Fail_WhenNameTooLong()
    {
        var request = new RoleRequest { Name = new string('A', 51) };
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Fail_WhenDescriptionTooLong()
    {
        var request = new RoleRequest { Name = "Valid", Description = new string('A', 501) };
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Description);
    }
}
