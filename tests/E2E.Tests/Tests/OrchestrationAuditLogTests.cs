using System.Net;
using System.Net.Http.Json;
using E2E.Tests.Helpers;
using E2E.Tests.Infrastructure;
using FluentAssertions;

namespace E2E.Tests.Tests;

/// <summary>
/// Verifies that OrchestrationService publishes IAuditLogCreated for workflow events
/// and that AuditService persists them.
/// </summary>
public sealed class OrchestrationAuditLogTests : E2ETestBase
{
    public OrchestrationAuditLogTests(SharedInfrastructureFixture infrastructure)
        : base(infrastructure) { }

    [Fact]
    public async Task WorkflowEvents_AuditServicePersistsWorkflowStartedAndStepRecorded()
    {
        // Arrange
        var payload = new
        {
            focus = "Orchestration audit test",
            @object = "E2E test object",
            activeFrom = DateTime.UtcNow.Date,
            activeTo = DateTime.UtcNow.Date.AddYears(1),
            magnitude = "100%",
            constraints = "none",
            status = "Draft",
            baselineProbability = 0.5m,
            departmentId = Guid.NewGuid(),
        };

        // Act
        var createResponse = await GoalClient.PostAsJsonAsync("/api/v1/Goal", payload);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.ReadAs<GoalDto>();

        // We need the Saga ID (which is != Goal ID) to poll audit logs correctly
        Guid sagaId = Guid.Empty;
        await PollingAssert.WaitUntilAsync(
            async () =>
            {
                var r = await OrchestrationClient.GetAsync($"/api/v1/Workflow/{created.Id}");
                if (!r.IsSuccessStatusCode) return false;
                var workflow = await r.ReadAs<WorkflowDto>();
                sagaId = workflow.Id;
                return true;
            },
            $"OrchestrationService to create saga for Goal/{created.Id}",
            timeout: TimeSpan.FromSeconds(10));

        // Assert — poll AuditService for WorkflowStarted and StepRecorded_GoalCreated
        await PollingAssert.WaitUntilAsync(
            async () =>
            {
                var r = await AuditClient.GetAsync($"/api/v1/AuditLog/SagaWorkflow/{sagaId}");
                if (!r.IsSuccessStatusCode) return false;
                var page = await r.ReadAs<PaginatedResponse<AuditLogDto>>();
                return page.Items.Any(a => a.Action == "WorkflowStarted")
                    && page.Items.Any(a => a.Action == "StepRecorded_GoalCreated");
            },
            $"AuditService to receive WorkflowStarted and StepRecorded_GoalCreated for SagaWorkflow/{sagaId}",
            timeout: TimeSpan.FromSeconds(30));
    }
}
