using UserService.Application.DTOs;
using UserService.Application.Validators;
using FluentValidation.TestHelper;

namespace UserService.Tests.Validators;

public class ResetPasswordRequestValidatorTests
{
    private readonly ResetPasswordRequestValidator _sut = new();

    [Fact]
    public void Should_Pass_WhenValid()
    {
        var request = new ResetPasswordRequest
        {
            Token = "reset-token",
            NewPassword = "NewPassword@1!",
            ConfirmNewPassword = "NewPassword@1!"
        };
        _sut.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_WhenTokenEmpty()
    {
        var request = new ResetPasswordRequest { Token = "", NewPassword = "NewPassword@1!", ConfirmNewPassword = "NewPassword@1!" };
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Token);
    }

    [Fact]
    public void Should_Fail_WhenNewPasswordTooShort()
    {
        var request = new ResetPasswordRequest { Token = "token", NewPassword = "Sh@1", ConfirmNewPassword = "Sh@1" };
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void Should_Fail_WhenPasswordsDoNotMatch()
    {
        var request = new ResetPasswordRequest
        {
            Token = "token",
            NewPassword = "NewPassword@1!",
            ConfirmNewPassword = "Different@1!"
        };
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.ConfirmNewPassword);
    }

    [Fact]
    public void Should_Fail_WhenNewPasswordNoSpecialChar()
    {
        var request = new ResetPasswordRequest
        {
            Token = "token",
            NewPassword = "Password1",
            ConfirmNewPassword = "Password1"
        };
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.NewPassword);
    }
}
