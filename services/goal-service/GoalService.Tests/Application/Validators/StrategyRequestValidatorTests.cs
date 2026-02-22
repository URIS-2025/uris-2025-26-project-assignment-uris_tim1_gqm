using FluentValidation.TestHelper;
using GoalService.Application.DTOs;
using GoalService.Application.Validators;
using System;
using Xunit;

namespace GoalService.Tests.Application.Validators;

public class StrategyRequestValidatorTests
{
    private readonly StrategyRequestValidator _validator;

    public StrategyRequestValidatorTests()
    {
        _validator = new StrategyRequestValidator();
    }

    [Fact]
    public void ValidRequest_ShouldNotHaveAnyValidationErrors()
    {
        var request = new StrategyRequest
        {
            Name = "A valid strategy",
            Description = "A valid description",
            Effectiveness = "High",
            RefinementType = "AND",
            GoalId = Guid.NewGuid()
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void InvalidName_ShouldHaveValidationError(string name)
    {
        var request = new StrategyRequest { Name = name };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void InvalidEffectiveness_ShouldHaveValidationError()
    {
        var request = new StrategyRequest { Effectiveness = "InvalidEffectiveness" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Effectiveness)
              .WithErrorMessage("Effectiveness must be one of: Low, Medium, High, VeryHigh.");
    }

    [Fact]
    public void EmptyEffectiveness_ShouldHaveValidationError()
    {
        var request = new StrategyRequest { Effectiveness = "" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Effectiveness)
              .WithErrorMessage("Effectiveness is required.");
    }
    
    [Fact]
    public void InvalidRefinementType_ShouldHaveValidationError()
    {
        var request = new StrategyRequest { RefinementType = "XOR" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.RefinementType)
              .WithErrorMessage("RefinementType must be one of: AND, OR.");
    }

    [Fact]
    public void EmptyRefinementType_ShouldHaveValidationError()
    {
        var request = new StrategyRequest { RefinementType = "" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.RefinementType)
              .WithErrorMessage("RefinementType is required.");
    }

    [Fact]
    public void EmptyGoalId_ShouldHaveValidationError()
    {
        var request = new StrategyRequest { GoalId = Guid.Empty };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.GoalId);
    }
}
