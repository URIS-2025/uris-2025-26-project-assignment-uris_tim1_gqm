using System.Net;
using System.Net.Http.Json;
using E2E.Tests.Helpers;
using E2E.Tests.Infrastructure;
using FluentAssertions;

namespace E2E.Tests.Tests;

/// <summary>
/// Critical path: creating a Goal triggers asynchronous messaging across three services.
///
/// Flow under test:
///   POST /api/Goal (GoalService)
///     → publishes IAuditLogCreated      → AuditService.AuditLogCreatedConsumer persists it
///     → publishes IGoalDomainEvent      (no consumer in scope, fire-and-forget)
///     → publishes IWorkflowTransitionRequested (StepName=StartWorkflow)
///     → publishes IWorkflowTransitionRequested (StepName=GoalCreated)
///         → OrchestrationService.WorkflowTransitionRequestedConsumer creates + records step
///             → publishes IAuditLogCreated twice (WorkflowStarted, StepRecorded)
///                 → AuditService persists both
/// </summary>
public sealed class GoalCreationMessagingTests : E2ETestBase
{
    public GoalCreationMessagingTests(SharedInfrastructureFixture infrastructure)
        : base(infrastructure) { }

    // ─── Test 1 ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateGoal_Returns201_AndGoalIsRetrievable()
    {
        // Arrange
        var payload = BuildGoalPayload("HTTP persistence smoke test");

        // Act
        var createResponse = await GoalClient.PostAsJsonAsync("/api/Goal", payload);

        // Assert — HTTP layer
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.ReadAs<GoalDto>();
        created.Focus.Should().Be("HTTP persistence smoke test");

        // Assert — DB read-back via GET
        var getResponse = await GoalClient.GetAsync($"/api/Goal/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.ReadAs<GoalDto>();
        fetched.Id.Should().Be(created.Id);
    }

    // ─── Test 2 ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateGoal_PublishesIAuditLogCreated_AuditServicePersistsIt()
    {
        // Arrange
        var payload = BuildGoalPayload("Audit messaging test");

        // Act — create goal (publishes IAuditLogCreated)
        var createResponse = await GoalClient.PostAsJsonAsync("/api/Goal", payload);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.ReadAs<GoalDto>();

        // Assert — poll AuditService until the GoalCreated entry appears
        await PollingAssert.WaitUntilAsync(
            async () =>
            {
                var r = await AuditClient.GetAsync($"/audit/Goal/{created.Id}");
                if (!r.IsSuccessStatusCode) return false;
                var page = await r.ReadAs<PaginatedResponse<AuditLogDto>>();
                return page.Items.Any(a =>
                    a.EntityId  == created.Id &&
                    a.Service   == "goal-service" &&
                    a.Action    == "GoalCreated");
            },
            $"AuditService to receive GoalCreated for Goal/{created.Id}",
            timeout: TimeSpan.FromSeconds(15));
    }

    // ─── Test 3 ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateGoal_PublishesIWorkflowTransitionRequested_OrchestrationStartsWorkflow()
    {
        // Arrange
        var payload = BuildGoalPayload("Workflow messaging test");

        // Act
        var createResponse = await GoalClient.PostAsJsonAsync("/api/Goal", payload);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.ReadAs<GoalDto>();

        // Assert — poll OrchestrationService until workflow is in expected final state
        WorkflowDto? workflow = null;

        await PollingAssert.WaitUntilAsync(
            async () =>
            {
                var r = await OrchestrationClient.GetAsync($"/workflow/{created.Id}");
                if (!r.IsSuccessStatusCode) return false;
                workflow = await r.ReadAs<WorkflowDto>();
                // Final state: InProgress with at least the GoalCreated step recorded
                return workflow.Status      == "InProgress"
                    && workflow.CurrentStep == "GoalCreated"
                    && workflow.Steps.Any(s => s.StepName == "GoalCreated" && s.Status == "Completed");
            },
            $"OrchestrationService workflow for Goal/{created.Id} to reach InProgress/GoalCreated",
            timeout: TimeSpan.FromSeconds(15));

        workflow.Should().NotBeNull();
        workflow!.GoalId.Should().Be(created.Id);
        workflow.Steps.Should().ContainSingle(s => s.StepName == "GoalCreated");
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static object BuildGoalPayload(string focus) => new
    {
        focus,
        @object          = "E2E test object",
        activeFrom       = DateTime.UtcNow.Date,
        activeTo         = DateTime.UtcNow.Date.AddYears(1),
        magnitude        = "100%",
        constraints      = "none",
        status           = "Draft",
        baselineProbability = 0.5m,
        departmentId     = Guid.NewGuid(),
    };
}
