using E2E.Tests.Factories;
using E2E.Tests.Infrastructure;

namespace E2E.Tests;

/// <summary>
/// Base class for all e2e test classes.
///
/// Creates one set of <c>WebApplicationFactory</c> instances per test-class lifetime —
/// each backed by fresh, isolated databases. Tests within a class share the same in-process
/// servers but use distinct GUIDs for their data so they don't interfere with each other.
/// </summary>
[Collection("Infrastructure")]
public abstract class E2ETestBase : IAsyncLifetime
{
    protected readonly SharedInfrastructureFixture Infrastructure;

    private GoalServiceFactory         _goalFactory         = null!;
    private AuditServiceFactory        _auditFactory        = null!;
    private OrchestrationServiceFactory _orchestrationFactory = null!;

    /// <summary>HTTP client for GoalService (in-process TestServer).</summary>
    protected HttpClient GoalClient         { get; private set; } = null!;
    /// <summary>HTTP client for AuditService (in-process TestServer).</summary>
    protected HttpClient AuditClient        { get; private set; } = null!;
    /// <summary>HTTP client for OrchestrationService (in-process TestServer).</summary>
    protected HttpClient OrchestrationClient { get; private set; } = null!;

    protected E2ETestBase(SharedInfrastructureFixture infrastructure)
    {
        Infrastructure = infrastructure;
    }

    public async Task InitializeAsync()
    {
        // Unique suffix so each test class gets independent DB schemas.
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var goalDbName = $"goaldb_{suffix}";
        var auditDbName = $"auditdb_{suffix}";
        var orchDbName = $"orchdb_{suffix}";

        await Infrastructure.EnsureDatabaseExistsAsync(goalDbName);
        await Infrastructure.EnsureDatabaseExistsAsync(auditDbName);
        await Infrastructure.EnsureDatabaseExistsAsync(orchDbName);

        _goalFactory = new GoalServiceFactory(
            Infrastructure.GetPostgresConnectionString(goalDbName),
            Infrastructure.RabbitMqUri,
            Infrastructure.RabbitMqUsername,
            Infrastructure.RabbitMqPassword);

        _auditFactory = new AuditServiceFactory(
            Infrastructure.GetPostgresConnectionString(auditDbName),
            Infrastructure.RabbitMqUri,
            Infrastructure.RabbitMqUsername,
            Infrastructure.RabbitMqPassword);

        _orchestrationFactory = new OrchestrationServiceFactory(
            Infrastructure.GetPostgresConnectionString(orchDbName),
            Infrastructure.RabbitMqUri,
            Infrastructure.RabbitMqUsername,
            Infrastructure.RabbitMqPassword);

        // CreateClient() forces the WebApplicationFactory to build and start the service.
        // The order matters: consumers must be started BEFORE publishers so no messages
        // are missed. Start consumers first, publisher last.
        AuditClient         = _auditFactory.CreateClient();
        OrchestrationClient = _orchestrationFactory.CreateClient();
        GoalClient          = _goalFactory.CreateClient();

        // Run DB migrations for services that don't auto-migrate outside Development.
        await _goalFactory.InitializeDatabaseAsync();
        await _orchestrationFactory.InitializeDatabaseAsync();
        // AuditService migrates unconditionally in Program.cs — no extra call needed.

        // Give consumers a moment to connect to RabbitMQ and declare queues.
        await Task.Delay(TimeSpan.FromSeconds(1));

        await OnInitializeAsync();
    }

    /// <summary>Optional hook for subclass-specific setup after base init.</summary>
    protected virtual Task OnInitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        GoalClient?.Dispose();
        AuditClient?.Dispose();
        OrchestrationClient?.Dispose();
        await _goalFactory.DisposeAsync();
        await _auditFactory.DisposeAsync();
        await _orchestrationFactory.DisposeAsync();
    }
}
