using AuditService.Application.DTOs;
using AuditService.Application.Validators;

namespace AuditService.Tests.Application.Validators;

public class CreateAuditLogRequestValidatorTests
{
    private readonly CreateAuditLogRequestValidator _validator = new();

    private static CreateAuditLogRequest ValidRequest() => new(
        ActorId: Guid.NewGuid(),
        ActorRole: "Admin",
        Service: "goal-service",
        Action: "GoalCreated",
        EntityType: "Goal",
        EntityId: Guid.NewGuid()
    );

    [Fact]
    public async Task Validate_ShouldPass_WhenRequestIsValid()
    {
        var result = await _validator.ValidateAsync(ValidRequest());

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenActorIdIsEmpty()
    {
        var request = ValidRequest() with { ActorId = Guid.Empty };

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ActorId");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenActorRoleIsEmpty()
    {
        var request = ValidRequest() with { ActorRole = "" };

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ActorRole");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenActorRoleExceedsMaxLength()
    {
        var request = ValidRequest() with { ActorRole = new string('A', 101) };

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ActorRole");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenServiceIsEmpty()
    {
        var request = ValidRequest() with { Service = "" };

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Service");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenServiceExceedsMaxLength()
    {
        var request = ValidRequest() with { Service = new string('s', 101) };

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Service");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenActionIsEmpty()
    {
        var request = ValidRequest() with { Action = "" };

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Action");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenActionExceedsMaxLength()
    {
        var request = ValidRequest() with { Action = new string('a', 101) };

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Action");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenEntityTypeIsEmpty()
    {
        var request = ValidRequest() with { EntityType = "" };

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "EntityType");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenEntityTypeExceedsMaxLength()
    {
        var request = ValidRequest() with { EntityType = new string('e', 101) };

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "EntityType");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenEntityIdIsEmpty()
    {
        var request = ValidRequest() with { EntityId = Guid.Empty };

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "EntityId");
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenMetadataIsNull()
    {
        var request = ValidRequest() with { Metadata = null };

        var result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenActorRoleIsExactlyMaxLength()
    {
        var request = ValidRequest() with { ActorRole = new string('A', 100) };

        var result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }
}
