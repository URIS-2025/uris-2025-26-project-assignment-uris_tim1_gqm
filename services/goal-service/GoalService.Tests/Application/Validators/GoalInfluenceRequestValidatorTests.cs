using FluentValidation.TestHelper;
using GoalService.Application.DTOs;
using GoalService.Application.Validators;
using System;
using Xunit;

namespace GoalService.Tests.Application.Validators;

public class GoalInfluenceRequestValidatorTests
{
    private readonly GoalInfluenceRequestValidator _validator;

    public GoalInfluenceRequestValidatorTests()
    {
        _validator = new GoalInfluenceRequestValidator();
    }

    [Fact]
    public void ValidRequest_ShouldNotHaveAnyValidationErrors()
    {
        var request = new GoalInfluenceRequest
        {
            GoalId = Guid.NewGuid(),
            StrategyId = Guid.NewGuid(),
            InfluenceType = "Positive",
            Strength = 0.8m,
            Confidence = 0.9m,
            Notes = "Valid note"
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void InvalidInfluenceType_ShouldHaveValidationError()
    {
        var request = new GoalInfluenceRequest { InfluenceType = "VeryPositive" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.InfluenceType)
              .WithErrorMessage("InfluenceType must be one of: Positive, Negative, Neutral.");
    }

    [Fact]
    public void EmptyInfluenceType_ShouldHaveValidationError()
    {
        var request = new GoalInfluenceRequest { InfluenceType = "" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.InfluenceType)
              .WithErrorMessage("InfluenceType is required.");
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void InvalidStrength_ShouldHaveValidationError(decimal strength)
    {
        var request = new GoalInfluenceRequest { Strength = strength };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Strength)
              .WithErrorMessage("Strength must be between 0.0 and 1.0.");
    }

    [Fact]
    public void EmptyIds_ShouldHaveValidationError()
    {
        var request = new GoalInfluenceRequest { GoalId = Guid.Empty, StrategyId = Guid.Empty };
        var result = _validator.TestValidate(request);
        
        result.ShouldHaveValidationErrorFor(x => x.GoalId);
        result.ShouldHaveValidationErrorFor(x => x.StrategyId);
    }
}
