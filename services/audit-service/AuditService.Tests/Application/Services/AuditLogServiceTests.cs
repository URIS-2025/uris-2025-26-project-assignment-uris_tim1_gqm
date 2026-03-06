using AuditService.Application.DTOs;
using AuditService.Application.Mappings;
using AuditService.Application.Services;
using AuditService.Infrastructure.Data;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Contracts;

namespace AuditService.Tests.Application.Services;

public class AuditLogServiceTests : IDisposable
{
    private readonly AuditDbContext _dbContext;
    private readonly AuditLogService _service;

    public AuditLogServiceTests()
    {
        var options = new DbContextOptionsBuilder<AuditDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AuditDbContext(options);

        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<AuditLogProfile>());
        var mapper = mapperConfig.CreateMapper();

        _service = new AuditLogService(_dbContext, mapper, NullLogger<AuditLogService>.Instance);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    private static CreateAuditLogRequest BuildRequest(
        Guid? actorId = null,
        string actorRole = "Admin",
        string service = "goal-service",
        string action = "GoalCreated",
        string entityType = "Goal",
        Guid? entityId = null,
        object? metadata = null)
    {
        return new CreateAuditLogRequest(
            ActorId: actorId ?? Guid.NewGuid(),
            ActorRole: actorRole,
            Service: service,
            Action: action,
            EntityType: entityType,
            EntityId: entityId ?? Guid.NewGuid(),
            Metadata: metadata
        );
    }

    // ─── CreateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ShouldReturnResponse_WithCorrectFields()
    {
        var request = BuildRequest();

        var result = await _service.CreateAsync(request);

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result!.Id);
        Assert.Equal(request.ActorId, result.ActorId);
        Assert.Equal(request.ActorRole, result.ActorRole);
        Assert.Equal(request.Service, result.Service);
        Assert.Equal(request.Action, result.Action);
        Assert.Equal(request.EntityType, result.EntityType);
        Assert.Equal(request.EntityId, result.EntityId);
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistToDatabase()
    {
        var request = BuildRequest();

        var result = await _service.CreateAsync(request);

        var persisted = await _dbContext.AuditLogs.FindAsync(result!.Id);
        Assert.NotNull(persisted);
        Assert.Equal(request.ActorId, persisted!.ActorId);
        Assert.Equal(request.Action, persisted.Action);
    }

    [Fact]
    public async Task CreateAsync_ShouldSerializeMetadata_WhenProvided()
    {
        var metadata = new { key = "value", count = 42 };
        var request = BuildRequest(metadata: metadata);

        var result = await _service.CreateAsync(request);

        var persisted = await _dbContext.AuditLogs.FindAsync(result!.Id);
        Assert.NotNull(persisted!.Metadata);
        Assert.Contains("value", persisted.Metadata);
    }

    [Fact]
    public async Task CreateAsync_ShouldSetTimestamp_ToUtcNow()
    {
        var before = DateTime.UtcNow;
        var request = BuildRequest();

        var result = await _service.CreateAsync(request);

        Assert.True(result!.Timestamp >= before);
        Assert.True(result.Timestamp <= DateTime.UtcNow);
    }

    // ─── GetAllAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllLogs_WhenNoFilterApplied()
    {
        await _service.CreateAsync(BuildRequest(service: "goal-service", action: "GoalCreated"));
        await _service.CreateAsync(BuildRequest(service: "user-service", action: "UserLoggedIn"));

        var result = await _service.GetAllAsync(new AuditLogFilter(), new PaginationRequest { PageNumber = 1, PageSize = 10 });

        Assert.Equal(2, result.Total);
        Assert.Equal(2, result.Items.Count());
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByService()
    {
        await _service.CreateAsync(BuildRequest(service: "goal-service"));
        await _service.CreateAsync(BuildRequest(service: "user-service"));

        var filter = new AuditLogFilter { Service = "goal-service" };
        var result = await _service.GetAllAsync(filter, new PaginationRequest { PageNumber = 1, PageSize = 10 });

        Assert.Equal(1, result.Total);
        Assert.All(result.Items, item => Assert.Equal("goal-service", item.Service));
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByAction()
    {
        await _service.CreateAsync(BuildRequest(action: "GoalCreated"));
        await _service.CreateAsync(BuildRequest(action: "GoalDeleted"));

        var filter = new AuditLogFilter { Action = "GoalCreated" };
        var result = await _service.GetAllAsync(filter, new PaginationRequest { PageNumber = 1, PageSize = 10 });

        Assert.Equal(1, result.Total);
        Assert.All(result.Items, item => Assert.Equal("GoalCreated", item.Action));
    }

    [Fact]
    public async Task GetAllAsync_ShouldFilterByActorId()
    {
        var actorId = Guid.NewGuid();
        await _service.CreateAsync(BuildRequest(actorId: actorId));
        await _service.CreateAsync(BuildRequest());

        var filter = new AuditLogFilter { ActorId = actorId };
        var result = await _service.GetAllAsync(filter, new PaginationRequest { PageNumber = 1, PageSize = 10 });

        Assert.Equal(1, result.Total);
        Assert.All(result.Items, item => Assert.Equal(actorId, item.ActorId));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoLogsExist()
    {
        var result = await _service.GetAllAsync(new AuditLogFilter(), new PaginationRequest { PageNumber = 1, PageSize = 10 });

        Assert.Equal(0, result.Total);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetAllAsync_ShouldRespectPagination()
    {
        for (int i = 0; i < 5; i++)
            await _service.CreateAsync(BuildRequest());

        var result = await _service.GetAllAsync(new AuditLogFilter(), new PaginationRequest { PageNumber = 1, PageSize = 2 });

        Assert.Equal(5, result.Total);
        Assert.Equal(2, result.Items.Count());
    }

    // ─── GetByEntityAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetByEntityAsync_ShouldReturnMatchingLogs()
    {
        var entityId = Guid.NewGuid();
        await _service.CreateAsync(BuildRequest(entityType: "Goal", entityId: entityId));
        await _service.CreateAsync(BuildRequest(entityType: "Goal", entityId: entityId));
        await _service.CreateAsync(BuildRequest(entityType: "Goal"));

        var result = await _service.GetByEntityAsync("Goal", entityId, new PaginationRequest { PageNumber = 1, PageSize = 10 });

        Assert.Equal(2, result.Total);
        Assert.All(result.Items, item => Assert.Equal(entityId, item.EntityId));
    }

    [Fact]
    public async Task GetByEntityAsync_ShouldReturnEmpty_WhenNoMatchingLogs()
    {
        var result = await _service.GetByEntityAsync("Goal", Guid.NewGuid(), new PaginationRequest { PageNumber = 1, PageSize = 10 });

        Assert.Equal(0, result.Total);
    }

    // ─── GetByActorAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetByActorAsync_ShouldReturnLogsForActor()
    {
        var actorId = Guid.NewGuid();
        await _service.CreateAsync(BuildRequest(actorId: actorId));
        await _service.CreateAsync(BuildRequest(actorId: actorId));
        await _service.CreateAsync(BuildRequest());

        var result = await _service.GetByActorAsync(actorId, new PaginationRequest { PageNumber = 1, PageSize = 10 });

        Assert.Equal(2, result.Total);
        Assert.All(result.Items, item => Assert.Equal(actorId, item.ActorId));
    }

    [Fact]
    public async Task GetByActorAsync_ShouldReturnEmpty_WhenActorHasNoLogs()
    {
        var result = await _service.GetByActorAsync(Guid.NewGuid(), new PaginationRequest { PageNumber = 1, PageSize = 10 });

        Assert.Equal(0, result.Total);
    }

    // ─── GetByServiceAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetByServiceAsync_ShouldReturnLogsForService()
    {
        await _service.CreateAsync(BuildRequest(service: "premise-service"));
        await _service.CreateAsync(BuildRequest(service: "premise-service"));
        await _service.CreateAsync(BuildRequest(service: "goal-service"));

        var result = await _service.GetByServiceAsync("premise-service", new PaginationRequest { PageNumber = 1, PageSize = 10 });

        Assert.Equal(2, result.Total);
        Assert.All(result.Items, item => Assert.Equal("premise-service", item.Service));
    }

    [Fact]
    public async Task GetByServiceAsync_ShouldReturnEmpty_WhenServiceHasNoLogs()
    {
        var result = await _service.GetByServiceAsync("unknown-service", new PaginationRequest { PageNumber = 1, PageSize = 10 });

        Assert.Equal(0, result.Total);
    }
}
