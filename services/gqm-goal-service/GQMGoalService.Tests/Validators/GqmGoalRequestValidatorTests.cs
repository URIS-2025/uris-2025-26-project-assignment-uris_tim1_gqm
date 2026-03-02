using FluentAssertions;
using GQMGoalService.Application.DTOs.GqmGoal;
using GQMGoalService.Application.Validators;

namespace GQMGoalService.Tests.Validators;

public class GqmGoalRequestValidatorTests
{
    private readonly GqmGoalRequestValidator _validator;

    public GqmGoalRequestValidatorTests()
    {
        _validator = new GqmGoalRequestValidator();
    }

    [Fact]
    public void Validate_ValidRequest_ShouldNotHaveErrors()
    {
        var request = new GqmGoalRequest { Description = "Valid Description", GoalId = Guid.NewGuid() };
        var result = _validator.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyDescription_ShouldHaveError()
    {
        var request = new GqmGoalRequest { Description = string.Empty, GoalId = Guid.NewGuid() };
        var result = _validator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }
}
