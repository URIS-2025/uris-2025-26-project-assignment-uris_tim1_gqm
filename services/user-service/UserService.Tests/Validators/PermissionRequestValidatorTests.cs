using UserService.Application.DTOs;
using UserService.Application.Validators;
using FluentValidation.TestHelper;

namespace UserService.Tests.Validators;

public class PermissionRequestValidatorTests
{
    private readonly PermissionRequestValidator _sut = new();

    [Fact]
    public void Should_Pass_WhenValid()
    {
        var request = new PermissionRequest { Name = "manage_users", Description = "Desc" };
        _sut.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_WhenNameEmpty()
    {
        var request = new PermissionRequest { Name = "" };
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Fail_WhenNameTooLong()
    {
        var request = new PermissionRequest { Name = new string('A', 101) };
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Name);
    }
}
