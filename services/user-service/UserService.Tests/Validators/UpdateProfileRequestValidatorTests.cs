using UserService.Application.DTOs;
using UserService.Application.Validators;
using FluentValidation.TestHelper;
namespace UserService.Tests.Validators;

public class UpdateProfileRequestValidatorTests
{
    private readonly UpdateProfileRequestValidator _sut = new();

    [Fact]
    public void Should_Pass_WhenValid()
    {
        var request = new UpdateProfileRequest { FirstName = "John", LastName = "Doe" };
        _sut.TestValidate(request).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_WhenFirstNameEmpty()
    {
        var request = new UpdateProfileRequest { FirstName = "", LastName = "Doe" };
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Should_Fail_WhenLastNameEmpty()
    {
        var request = new UpdateProfileRequest { FirstName = "John", LastName = "" };
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void Should_Fail_WhenFirstNameTooLong()
    {
        var request = new UpdateProfileRequest { FirstName = new string('A', 101), LastName = "Doe" };
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Should_Fail_WhenLastNameTooLong()
    {
        var request = new UpdateProfileRequest { FirstName = "John", LastName = new string('A', 101) };
        _sut.TestValidate(request).ShouldHaveValidationErrorFor(x => x.LastName);
    }
}
