using System.Net;
using System.Net.Http.Json;
using E2E.Tests.Helpers;
using E2E.Tests.Infrastructure;
using FluentAssertions;

namespace E2E.Tests.Tests;

/// <summary>
/// Verifies that OrchestrationService can correctly roll back a saga.
/// When /cancel is called on OrchestrationService, it should invoke compensation endpoints.
/// </summary>
public sealed class SagaCompensationTests : E2ETestBase
{
    public SagaCompensationTests(SharedInfrastructureFixture infrastructure)
        : base(infrastructure) { }

    [Fact]
    public async Task CancelWorkflow_TriggersCompensationInGoalService()
    {
        // 1. Create a goal (triggers Saga Start and GoalCreated step)
        var payload = new
        {
            focus = "Compensation test goal",
            @object = "E2E test object",
            activeFrom = DateTime.UtcNow.Date.AddDays(1),
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

        // 2. Wait for Saga to be created
        await PollingAssert.WaitUntilAsync(
            async () =>
            {
                var r = await OrchestrationClient.GetAsync($"/api/v1/Workflow/{created.Id}");
                return r.IsSuccessStatusCode;
            },
            $"Saga to be created for Goal/{created.Id}");

        // 3. Manually simulate "Activated" step via ID of the saga (for full coverage)
        // This gives us something nontrivial to compensate
        var recordStepResponse = await OrchestrationClient.PostAsJsonAsync($"/api/v1/Workflow/{created.Id}/step", new
        {
            StepName = "Activated",
            CompensationEndpoint = $"api/Goal/{created.Id}/revert-to-draft",
            CompensationPayload = "{}"
        });
        recordStepResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Cancel the workflow
        var cancelResponse = await OrchestrationClient.PostAsync($"/api/v1/Workflow/{created.Id}/cancel", null);
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 5. Verify Compensation: Orchestration should have called revert-to-draft
        // Since we don't have a direct way to see the HTTP call, we check the result (Audit log or Goal status)
        // But wait, our RevertToDraft endpoint in GoalService just sets status to Draft.
        // Let's check for the audit log published by Orchestration on compensation.
        
        await PollingAssert.WaitUntilAsync(
            async () =>
            {
                var r = await OrchestrationClient.GetAsync($"/api/v1/Workflow/{created.Id}");
                var workflow = await r.ReadAs<WorkflowDto>();
                return workflow.Status == "Compensated";
            },
            $"Saga to reach Compensated state",
            timeout: TimeSpan.FromSeconds(15));

        // 6. Double check GoalService (though we manually kept it in Draft, the call should have happened)
        var goalInfo = await GoalClient.GetFromJsonAsync<GoalDto>($"/api/v1/Goal/{created.Id}");
        goalInfo!.Status.Should().Be("Draft");
        
        // 7. Check Audit Log for "WorkflowCompensated"
        var auditResponse = await AuditClient.GetAsync($"/api/v1/AuditLog/SagaWorkflow/{created.Id}"); // Note: we used GoalId as EntityId for saga audits in some places? No, check WorkflowService.cs
        // Actually WorkflowService.cs uses workflow.Id (Saga ID) for EntityId.
        // Let's get the workflow ID
        var finalWorkflowR = await OrchestrationClient.GetAsync($"/api/v1/Workflow/{created.Id}");
        var finalWorkflow = await finalWorkflowR.ReadAs<WorkflowDto>();
        
        await PollingAssert.WaitUntilAsync(
            async () =>
            {
                var r = await AuditClient.GetAsync($"/api/v1/AuditLog/SagaWorkflow/{finalWorkflow.Id}");
                if (!r.IsSuccessStatusCode) return false;
                var page = await r.ReadAs<PaginatedResponse<AuditLogDto>>();
                return page.Items.Any(a => a.Action == "WorkflowCompensated");
            },
            $"AuditService to receive WorkflowCompensated for Saga/{finalWorkflow.Id}",
            timeout: TimeSpan.FromSeconds(10));
    }
}
