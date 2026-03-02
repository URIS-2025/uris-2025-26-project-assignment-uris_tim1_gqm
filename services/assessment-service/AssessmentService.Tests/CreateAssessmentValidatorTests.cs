using AssessmentService.Application.DTOs;
using AssessmentService.Application.Validators;
using AssessmentService.Domain.Enums;

namespace AssessmentService.Tests;

public class CreateAssessmentValidatorTests
{
    private readonly CreateAssessmentValidator _validator = new();

    [Fact]
    public async Task Validate_ShouldPass_WhenRequestIsValid()
    {
        var request = new CreateAssessmentRequest(
            GoalId: Guid.NewGuid(),
            Probability: 0.75m,
            State: AssessmentState.Draft,
            Method: AssessmentMethod.Expert,
            Notes: "Valid notes"
        );

        var result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenGoalIdIsEmpty()
    {
        var request = new CreateAssessmentRequest(
            GoalId: Guid.Empty,
            Probability: 0.50m,
            State: AssessmentState.Draft,
            Method: AssessmentMethod.Expert,
            Notes: ""
        );

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "GoalId");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenProbabilityIsBelowZero()
    {
        var request = new CreateAssessmentRequest(
            GoalId: Guid.NewGuid(),
            Probability: -0.1m,
            State: AssessmentState.Draft,
            Method: AssessmentMethod.Expert,
            Notes: ""
        );

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Probability");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenProbabilityIsAboveOne()
    {
        var request = new CreateAssessmentRequest(
            GoalId: Guid.NewGuid(),
            Probability: 1.1m,
            State: AssessmentState.Draft,
            Method: AssessmentMethod.Expert,
            Notes: ""
        );

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Probability");
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenProbabilityIsBoundaryZero()
    {
        var request = new CreateAssessmentRequest(
            GoalId: Guid.NewGuid(),
            Probability: 0.0m,
            State: AssessmentState.Draft,
            Method: AssessmentMethod.Expert,
            Notes: ""
        );

        var result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenProbabilityIsBoundaryOne()
    {
        var request = new CreateAssessmentRequest(
            GoalId: Guid.NewGuid(),
            Probability: 1.0m,
            State: AssessmentState.InProgress,
            Method: AssessmentMethod.DataDriven,
            Notes: ""
        );

        var result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenNotesExceedMaxLength()
    {
        var request = new CreateAssessmentRequest(
            GoalId: Guid.NewGuid(),
            Probability: 0.50m,
            State: AssessmentState.Draft,
            Method: AssessmentMethod.Expert,
            Notes: new string('x', 2001)
        );

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Notes");
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenNotesAreExactlyAtMaxLength()
    {
        var request = new CreateAssessmentRequest(
            GoalId: Guid.NewGuid(),
            Probability: 0.50m,
            State: AssessmentState.Draft,
            Method: AssessmentMethod.Expert,
            Notes: new string('x', 2000)
        );

        var result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }
}
