using FluentAssertions;
using OrchestrationService.Application.DTOs;
using OrchestrationService.Application.Validators;

namespace OrchestrationService.Tests.Application.Validators;

public class StartWorkflowRequestValidatorTests
{
    private readonly StartWorkflowRequestValidator _validator = new();

    [Fact]
    public async Task Validate_ValidRequest_ShouldPass()
    {
        var request = new StartWorkflowRequest { GoalId = Guid.NewGuid() };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_EmptyGoalId_ShouldFail()
    {
        var request = new StartWorkflowRequest { GoalId = Guid.Empty };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "GoalId");
    }
}
