using UserService.Application.DTOs;
using UserService.Application.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace UserService.Tests.Validators;

public class UserRequestValidatorTests
{
    private readonly UserRequestValidator _sut = new();

    [Fact]
    public void Should_Pass_WhenValidRequest()
    {
        var request = new UserRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@test.com",
            Password = "Password@1"
        };

        var result = _sut.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_WhenFirstNameEmpty()
    {
        var request = new UserRequest { FirstName = "", LastName = "Doe", Email = "a@b.com", Password = "Password@1" };
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Should_Fail_WhenLastNameEmpty()
    {
        var request = new UserRequest { FirstName = "John", LastName = "", Email = "a@b.com", Password = "Password@1" };
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void Should_Fail_WhenEmailInvalid()
    {
        var request = new UserRequest { FirstName = "John", LastName = "Doe", Email = "not-an-email", Password = "Password@1" };
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Fail_WhenPasswordTooShort()
    {
        var request = new UserRequest { FirstName = "John", LastName = "Doe", Email = "a@b.com", Password = "Sh@1" };
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_WhenPasswordNoUppercase()
    {
        var request = new UserRequest { FirstName = "John", LastName = "Doe", Email = "a@b.com", Password = "password@1" };
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_WhenPasswordNoDigit()
    {
        var request = new UserRequest { FirstName = "John", LastName = "Doe", Email = "a@b.com", Password = "Password@" };
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_WhenPasswordNoSpecialChar()
    {
        var request = new UserRequest { FirstName = "John", LastName = "Doe", Email = "a@b.com", Password = "Password1" };
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Password);
    }
}
