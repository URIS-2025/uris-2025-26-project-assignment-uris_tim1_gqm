using System.Net;
using System.Net.Http.Json;
using E2E.Tests.Helpers;
using E2E.Tests.Infrastructure;
using FluentAssertions;

namespace E2E.Tests.Tests;

/// <summary>
/// Verifies that GoalService publishes IGoalDomainEvent and that it is fire-and-forget.
/// No consumer is registered in the test scope, so the queue should accumulate messages.
/// </summary>
public sealed class GoalDomainEventQueueTests : E2ETestBase
{
    public GoalDomainEventQueueTests(SharedInfrastructureFixture infrastructure)
        : base(infrastructure) { }

    [Fact]
    public async Task CreateGoal_PublishesIGoalDomainEvent_QueueHasMessages()
    {
        // Arrange
        var queueName = "GoalDomainEvent";
        var exchangeName = "Shared.Contracts.Messages:IGoalDomainEvent";
        var managementUrl = Infrastructure.RabbitMqManagementUri;
        
        using (var setupClient = new HttpClient())
        {
            var auth = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("guest:guest"));
            setupClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", auth);
            
            // 1. Create queue
            await setupClient.PutAsJsonAsync($"{managementUrl}/api/queues/%2F/{queueName}", new { durable = true, auto_delete = false });
            
            // 2. Create binding
            await setupClient.PostAsJsonAsync($"{managementUrl}/api/bindings/%2F/e/{exchangeName}/q/{queueName}", new { });
        }

        var payload = new
        {
            focus = "Domain event queue test",
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
        var createResponse = await GoalClient.PostAsJsonAsync("/api/Goal", payload);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.ReadAs<GoalDto>();

        // Assert — poll RabbitMQ management API for queue length
        var pollUrl = $"{managementUrl}/api/queues/%2F/{queueName}";

        await PollingAssert.WaitUntilAsync(
            async () =>
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("guest:guest")));
                var response = await client.GetAsync(pollUrl);
                if (!response.IsSuccessStatusCode) return false;
                var json = await response.Content.ReadAsStringAsync();
                return json.Contains("\"messages\":1") || json.Contains("\"messages\": 1"); // crude but effective
            },
            $"RabbitMQ {queueName} queue to have at least 1 message",
            timeout: TimeSpan.FromSeconds(15));
    }
}
