using UserService.Application.DTOs;
using UserService.Application.Validators;
using FluentValidation.TestHelper;

namespace UserService.Tests.Validators;

public class AssignRoleRequestValidatorTests
{
    private readonly AssignRoleRequestValidator _sut = new();

    [Fact]
    public void Should_Pass_WhenValid()
    {
        var request = new AssignRoleRequest
        {
            UserId = Guid.NewGuid(),
            RoleId = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid()
        };
        _sut.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_WhenUserIdEmpty()
    {
        var request = new AssignRoleRequest { UserId = Guid.Empty, RoleId = Guid.NewGuid(), OrganizationId = Guid.NewGuid() };
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Should_Fail_WhenRoleIdEmpty()
    {
        var request = new AssignRoleRequest { UserId = Guid.NewGuid(), RoleId = Guid.Empty, OrganizationId = Guid.NewGuid() };
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.RoleId);
    }

    [Fact]
    public void Should_Fail_WhenOrganizationIdEmpty()
    {
        var request = new AssignRoleRequest { UserId = Guid.NewGuid(), RoleId = Guid.NewGuid(), OrganizationId = Guid.Empty };
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.OrganizationId);
    }
}
