using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using PremiseService.Application.DTOs;
using PremiseService.Application.Interfaces;
using PremiseService.Application.Mappings;
using PremiseService.Application.Services;
using PremiseService.Domain.Entities;
using PremiseService.Domain.Enums;
using PremiseService.Domain.Exceptions;

namespace PremiseService.Tests;


public class PremiseAppServiceTests : IDisposable
{
    private readonly DbContext _dbContext;
    private readonly IPremiseService _service;
    private readonly IMapper _mapper;

    private static readonly Guid GoalId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
    private static readonly Guid StrategyId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");

    public PremiseAppServiceTests()
    {
        var options = new DbContextOptionsBuilder<TestPremiseDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var testContext = new TestPremiseDbContext(options);
        testContext.Database.EnsureCreated();

        _dbContext = testContext;

        var config = new MapperConfiguration(cfg => cfg.AddProfile<PremiseMappingProfile>());
        _mapper = config.CreateMapper();

        _service = new PremiseAppService(testContext, _mapper);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }



    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsPremiseResponse()
    {
        var request = new PremiseRequest("Test premise", PremiseType.Assumption, GoalId, StrategyId);

        var result = await _service.CreateAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Test premise", result.Description);
        Assert.Equal(PremiseType.Assumption, result.Type);
        Assert.True(result.IsActive);
        Assert.Null(result.NewVersionOf);
        Assert.Equal(GoalId, result.GoalId);
        Assert.Equal(StrategyId, result.StrategyId);
    }

    [Fact]
    public async Task CreateAsync_WithOnlyGoalId_StrategyIdIsNull()
    {
        var request = new PremiseRequest("Goal-only premise", PremiseType.Context, GoalId, null);

        var result = await _service.CreateAsync(request);

        Assert.Equal(GoalId, result.GoalId);
        Assert.Null(result.StrategyId);
    }



    [Fact]
    public async Task GetByIdAsync_ExistingPremise_ReturnsPremise()
    {
        var created = await _service.CreateAsync(
            new PremiseRequest("Test", PremiseType.Assumption, GoalId, null));

        var result = await _service.GetByIdAsync(created.Id);

        Assert.Equal(created.Id, result.Id);
        Assert.Equal("Test", result.Description);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<PremiseNotFoundException>(
            () => _service.GetByIdAsync(Guid.NewGuid()));
    }



    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedResult()
    {
        await _service.CreateAsync(new PremiseRequest("P1", PremiseType.Assumption, GoalId, null));
        await _service.CreateAsync(new PremiseRequest("P2", PremiseType.Context, null, StrategyId));

        var result = await _service.GetAllAsync(1, 10);

        Assert.Equal(2, result.Total);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.Size);
        Assert.Equal(2, result.Items.Count());
    }



    [Fact]
    public async Task GetActiveByGoalIdAsync_ExistingGoal_ReturnsActivePremises()
    {
        await _service.CreateAsync(new PremiseRequest("Active one", PremiseType.Assumption, GoalId, null));

        var result = await _service.GetActiveByGoalIdAsync(GoalId);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetActiveByGoalIdAsync_NoResults_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<PremisesNotFoundByGoalException>(
            () => _service.GetActiveByGoalIdAsync(Guid.NewGuid()));
    }



    [Fact]
    public async Task GetActiveByStrategyIdAsync_ExistingStrategy_ReturnsActivePremises()
    {
        await _service.CreateAsync(new PremiseRequest("Active one", PremiseType.Context, null, StrategyId));

        var result = await _service.GetActiveByStrategyIdAsync(StrategyId);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetActiveByStrategyIdAsync_NoResults_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<PremisesNotFoundByStrategyException>(
            () => _service.GetActiveByStrategyIdAsync(Guid.NewGuid()));
    }



    [Fact]
    public async Task UpdateAsync_ActivePremise_CreatesNewVersionAndDeactivatesOld()
    {
        var original = await _service.CreateAsync(
            new PremiseRequest("Original", PremiseType.Assumption, GoalId, StrategyId));

        var updateRequest = new PremiseUpdateRequest("Updated", PremiseType.Context, GoalId, null);
        var updated = await _service.UpdateAsync(original.Id, updateRequest);

        Assert.NotEqual(original.Id, updated.Id);
        Assert.Equal("Updated", updated.Description);
        Assert.Equal(PremiseType.Context, updated.Type);
        Assert.True(updated.IsActive);
        Assert.Equal(original.Id, updated.NewVersionOf);
        Assert.Equal(GoalId, updated.GoalId);
        Assert.Null(updated.StrategyId);

        var oldPremise = await _service.GetByIdAsync(original.Id);
        Assert.False(oldPremise.IsActive);
    }

    [Fact]
    public async Task UpdateAsync_AlreadyDeactivatedPremise_ThrowsConflict()
    {
        var original = await _service.CreateAsync(
            new PremiseRequest("Original", PremiseType.Assumption, GoalId, null));


        await _service.UpdateAsync(original.Id,
            new PremiseUpdateRequest("V2", PremiseType.Assumption, GoalId, null));

        await Assert.ThrowsAsync<PremiseAlreadyDeactivatedException>(
            () => _service.UpdateAsync(original.Id,
                new PremiseUpdateRequest("V3", PremiseType.Assumption, GoalId, null)));
    }

    [Fact]
    public async Task UpdateAsync_NonExistentPremise_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<PremiseNotFoundException>(
            () => _service.UpdateAsync(Guid.NewGuid(),
                new PremiseUpdateRequest("Test", PremiseType.Assumption, GoalId, null)));
    }



    [Fact]
    public async Task DeleteAsync_ExistingPremise_SetsInactive()
    {
        var created = await _service.CreateAsync(
            new PremiseRequest("To delete", PremiseType.Assumption, GoalId, null));

        await _service.DeleteAsync(created.Id);

        var deleted = await _service.GetByIdAsync(created.Id);
        Assert.False(deleted.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentPremise_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<PremiseNotFoundException>(
            () => _service.DeleteAsync(Guid.NewGuid()));
    }



    [Fact]
    public async Task UpdateAsync_VersioningChain_CreatesCorrectChain()
    {
        var a = await _service.CreateAsync(
            new PremiseRequest("Version A", PremiseType.Assumption, GoalId, null));

        var b = await _service.UpdateAsync(a.Id,
            new PremiseUpdateRequest("Version B", PremiseType.Assumption, GoalId, null));

        var c = await _service.UpdateAsync(b.Id,
            new PremiseUpdateRequest("Version C", PremiseType.Context, GoalId, StrategyId));


        Assert.Equal(a.Id, b.NewVersionOf);
        Assert.Equal(b.Id, c.NewVersionOf);

        var premiseA = await _service.GetByIdAsync(a.Id);
        var premiseB = await _service.GetByIdAsync(b.Id);
        Assert.False(premiseA.IsActive);
        Assert.False(premiseB.IsActive);
        Assert.True(c.IsActive);
    }
}


public class TestPremiseDbContext : DbContext, IPremiseDbContext
{
    public TestPremiseDbContext(DbContextOptions<TestPremiseDbContext> options) : base(options) { }

    public DbSet<Premise> Premises { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Premise>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Type).HasConversion<string>();
        });
    }
}
