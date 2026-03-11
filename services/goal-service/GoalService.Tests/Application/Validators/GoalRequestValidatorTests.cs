using FluentValidation.TestHelper;
using GoalService.Application.DTOs;
using GoalService.Application.Validators;
using System;
using Xunit;

namespace GoalService.Tests.Application.Validators;

public class GoalRequestValidatorTests
{
    private readonly GoalRequestValidator _validator;

    public GoalRequestValidatorTests()
    {
        _validator = new GoalRequestValidator();
    }

    [Fact]
    public void ValidRequest_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var request = new GoalRequest
        {
            Focus = "Improve quality",
            Object = "Product A",
            ActiveFrom = DateTime.UtcNow.AddDays(1),
            ActiveTo = DateTime.UtcNow.AddYears(1),
            Magnitude = "By 10%",
            Constraints = "Budget constraint",
            Status = "Draft",
            BaselineProbability = 0.5m,
            DepartmentId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void InvalidFocus_ShouldHaveValidationError(string focus)
    {
        var request = new GoalRequest { Focus = focus };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Focus);
    }

    [Fact]
    public void FocusExceedingMaxLength_ShouldHaveValidationError()
    {
        var request = new GoalRequest { Focus = new string('a', 501) };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Focus);
    }

    [Fact]
    public void ActiveTo_BeforeActiveFrom_ShouldHaveValidationError()
    {
        var request = new GoalRequest
        {
            ActiveFrom = DateTime.UtcNow.AddDays(5),
            ActiveTo = DateTime.UtcNow.AddDays(1) // ActiveTo is before ActiveFrom
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ActiveTo)
              .WithErrorMessage("ActiveTo must be after ActiveFrom.");
    }

    [Fact]
    public void EmptyDepartmentId_ShouldHaveValidationError()
    {
        var request = new GoalRequest { DepartmentId = Guid.Empty };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DepartmentId);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void InvalidBaselineProbability_ShouldHaveValidationError(decimal prob)
    {
        var request = new GoalRequest { BaselineProbability = prob };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.BaselineProbability);
    }

    [Fact]
    public void InvalidStatus_ShouldHaveValidationError()
    {
        var request = new GoalRequest { Status = "InvalidStatus" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Status)
              .WithErrorMessage("Status must be one of: Draft, Active, Completed, Cancelled.");
    }

    [Fact]
    public void EmptyStatus_ShouldHaveValidationError()
    {
        var request = new GoalRequest { Status = "" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Status)
              .WithErrorMessage("Status is required.");
    }
}
