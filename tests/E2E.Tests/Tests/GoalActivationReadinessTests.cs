using System.Net;
using System.Net.Http.Json;
using E2E.Tests.Helpers;
using E2E.Tests.Infrastructure;
using FluentAssertions;

namespace E2E.Tests.Tests;

/// <summary>
/// Verifies that GoalService correctly identifies activation readiness and blocks activation
/// when prerequisites (Assessment, GQM structure, Strategy) are missing.
/// </summary>
public sealed class GoalActivationReadinessTests : E2ETestBase
{
    public GoalActivationReadinessTests(SharedInfrastructureFixture infrastructure)
        : base(infrastructure) { }

    [Fact]
    public async Task NewGoal_IsInitiallyNotReady_AndBlocksActivation()
    {
        // 1. Create a fresh goal
        var payload = new
        {
            focus = "Readiness test goal",
            @object = "E2E test object",
            activeFrom = DateTime.UtcNow.Date,
            activeTo = DateTime.UtcNow.Date.AddYears(1),
            magnitude = "100%",
            constraints = "none",
            status = "Draft",
            baselineProbability = 0.5m,
            departmentId = Guid.NewGuid(),
        };

        var createResponse = await GoalClient.PostAsJsonAsync("/api/v1/Goal", payload);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.ReadAs<GoalDto>();

        // 2. Check Readiness (should be false with 3 blockers)
        var readinessResponse = await GoalClient.GetAsync($"/api/v1/Goal/{created.Id}/readiness");
        readinessResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var readiness = await readinessResponse.ReadAs<ReadinessDto>();

        readiness.IsReady.Should().BeFalse();
        readiness.Blockers.Should().Contain(b => b.Contains("active strategy"));
        readiness.Blockers.Should().Contain(b => b.Contains("assessment"));
        readiness.Blockers.Should().Contain(b => b.Contains("GQM structure"));

        // 3. Attempt Activation (should return 422 UnprocessableEntity because readiness check fails)
        var activateResponse = await GoalClient.PostAsync($"/api/v1/Goal/{created.Id}/activate", null);
        activateResponse.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        
        // Final verification: status remains Draft
        var goalInfo = await GoalClient.GetFromJsonAsync<GoalDto>($"/api/v1/Goal/{created.Id}");
        goalInfo!.Status.Should().Be("Draft");
    }
}

public record ReadinessDto(bool IsReady, string[] Blockers);
