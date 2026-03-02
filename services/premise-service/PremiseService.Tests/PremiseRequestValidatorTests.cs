using FluentValidation.TestHelper;
using PremiseService.Application.DTOs;
using PremiseService.Application.Validators;
using PremiseService.Domain.Enums;

namespace PremiseService.Tests;


public class PremiseRequestValidatorTests
{
    private readonly PremiseRequestValidator _validator = new();
    private readonly PremiseUpdateRequestValidator _updateValidator = new();

    [Fact]
    public void ValidRequest_WithBothIds_ShouldPassValidation()
    {
        var request = new PremiseRequest(
            "Valid description",
            PremiseType.Assumption,
            Guid.NewGuid(),
            Guid.NewGuid());

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ValidRequest_WithOnlyGoalId_ShouldPassValidation()
    {
        var request = new PremiseRequest(
            "Valid description",
            PremiseType.Context,
            Guid.NewGuid(),
            null);

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ValidRequest_WithOnlyStrategyId_ShouldPassValidation()
    {
        var request = new PremiseRequest(
            "Valid description",
            PremiseType.Assumption,
            null,
            Guid.NewGuid());

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void InvalidRequest_WithNoIds_ShouldFailValidation()
    {
        var request = new PremiseRequest(
            "Valid description",
            PremiseType.Assumption,
            null,
            null);

        var result = _validator.TestValidate(request);
        result.ShouldHaveAnyValidationError()
            .WithErrorMessage("At least one of GoalId or StrategyId must be provided.");
    }

    [Fact]
    public void InvalidRequest_EmptyDescription_ShouldFailValidation()
    {
        var request = new PremiseRequest(
            "",
            PremiseType.Assumption,
            Guid.NewGuid(),
            null);

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void InvalidRequest_DescriptionTooLong_ShouldFailValidation()
    {
        var request = new PremiseRequest(
            new string('A', 1001),
            PremiseType.Assumption,
            Guid.NewGuid(),
            null);

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void ValidUpdateRequest_ShouldPassValidation()
    {
        var request = new PremiseUpdateRequest(
            "Updated description",
            PremiseType.Context,
            Guid.NewGuid(),
            null);

        var result = _updateValidator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void InvalidUpdateRequest_NoIds_ShouldFailValidation()
    {
        var request = new PremiseUpdateRequest(
            "Updated description",
            PremiseType.Context,
            null,
            null);

        var result = _updateValidator.TestValidate(request);
        result.ShouldHaveAnyValidationError()
            .WithErrorMessage("At least one of GoalId or StrategyId must be provided.");
    }

    [Fact]
    public void InvalidUpdateRequest_EmptyDescription_ShouldFailValidation()
    {
        var request = new PremiseUpdateRequest(
            "",
            PremiseType.Assumption,
            Guid.NewGuid(),
            null);

        var result = _updateValidator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
}
