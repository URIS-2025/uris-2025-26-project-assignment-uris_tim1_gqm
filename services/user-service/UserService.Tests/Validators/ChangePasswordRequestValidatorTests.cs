using UserService.Application.DTOs;
using UserService.Application.Validators;
using FluentValidation.TestHelper;

namespace UserService.Tests.Validators;

public class ChangePasswordRequestValidatorTests
{
    private readonly ChangePasswordRequestValidator _sut = new();

    [Fact]
    public void Should_Pass_WhenValid()
    {
        var request = new ChangePasswordRequest
        {
            OldPassword = "OldPassword@1",
            NewPassword = "NewPassword@1!",
            ConfirmNewPassword = "NewPassword@1!"
        };
        _sut.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_WhenOldPasswordEmpty()
    {
        var request = new ChangePasswordRequest { OldPassword = "", NewPassword = "NewPassword@1!", ConfirmNewPassword = "NewPassword@1!" };
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.OldPassword);
    }

    [Fact]
    public void Should_Fail_WhenNewPasswordTooShort()
    {
        var request = new ChangePasswordRequest { OldPassword = "Old@1234", NewPassword = "Sh@1", ConfirmNewPassword = "Sh@1" };
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void Should_Fail_WhenPasswordsDoNotMatch()
    {
        var request = new ChangePasswordRequest
        {
            OldPassword = "Old@1234",
            NewPassword = "NewPassword@1!",
            ConfirmNewPassword = "Different@1!"
        };
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.ConfirmNewPassword);
    }

    [Fact]
    public void Should_Fail_WhenNewPasswordNoSpecialChar()
    {
        var request = new ChangePasswordRequest
        {
            OldPassword = "Old12345",
            NewPassword = "Password1",
            ConfirmNewPassword = "Password1"
        };
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.NewPassword);
    }
}
