using FluentAssertions;
using OrchestrationService.Application.DTOs;
using OrchestrationService.Application.Validators;

namespace OrchestrationService.Tests.Application.Validators;

public class RecordStepRequestValidatorTests
{
    private readonly RecordStepRequestValidator _validator = new();

    [Fact]
    public async Task Validate_ValidRequest_ShouldPass()
    {
        var request = new RecordStepRequest
        {
            StepName = "GoalCreated",
            CompensationEndpoint = "api/Goal/123",
            CompensationPayload = "{}"
        };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_EmptyStepName_ShouldFail()
    {
        var request = new RecordStepRequest
        {
            StepName = "",
            CompensationEndpoint = "api/Goal/123",
            CompensationPayload = "{}"
        };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "StepName");
    }

    [Fact]
    public async Task Validate_EmptyCompensationEndpoint_ShouldFail()
    {
        var request = new RecordStepRequest
        {
            StepName = "GoalCreated",
            CompensationEndpoint = "",
            CompensationPayload = "{}"
        };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CompensationEndpoint");
    }

    [Fact]
    public async Task Validate_StepNameExceedsMaxLength_ShouldFail()
    {
        var request = new RecordStepRequest
        {
            StepName = new string('A', 101),
            CompensationEndpoint = "api/Goal/123",
            CompensationPayload = "{}"
        };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "StepName");
    }

    [Fact]
    public async Task Validate_EmptyPayload_ShouldPass()
    {
        // CompensationPayload is optional
        var request = new RecordStepRequest
        {
            StepName = "GoalCreated",
            CompensationEndpoint = "api/Goal/123",
            CompensationPayload = ""
        };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeTrue();
    }
}
