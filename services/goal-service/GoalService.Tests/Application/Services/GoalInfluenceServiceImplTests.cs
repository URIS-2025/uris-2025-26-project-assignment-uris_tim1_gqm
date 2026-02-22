using FluentAssertions;
using GoalService.Application.DTOs;
using GoalService.Infrastructure.Services;
using GoalService.Domain.Entities;
using GoalService.Domain.Enums;
using GoalService.Domain.Exceptions;
using GoalService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace GoalService.Tests.Application.Services;

public class GoalInfluenceServiceImplTests : IDisposable
{
    private readonly GoalDbContext _context;
    private readonly GoalInfluenceServiceImpl _service;

    public GoalInfluenceServiceImplTests()
    {
        var options = new DbContextOptionsBuilder<GoalDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new GoalDbContext(options);
        _service = new GoalInfluenceServiceImpl(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CreateAsync_ValidHierarchy_ShouldCreateInfluence()
    {
        // Arrange - Setup a valid Goal -> Strategy tree
        var rootGoalId = Guid.NewGuid();
        var rootStrategyId = Guid.NewGuid();
        var childGoalId = Guid.NewGuid();

        _context.Goals.Add(new Goal { Id = rootGoalId, Focus = "Root", Object = "O", Magnitude = "M", Constraints = "C", Status = GoalStatus.Draft, BaselineProbability = 0.5m, DepartmentId = Guid.NewGuid() });
        _context.Strategies.Add(new Strategy { Id = rootStrategyId, Name = "Strat", GoalId = rootGoalId, Effectiveness = EffectivenessLevel.High, RefinementType = RefinementType.AND, Description = "" });
        _context.Goals.Add(new Goal { Id = childGoalId, Focus = "Child", Object = "O", Magnitude = "M", Constraints = "C", Status = GoalStatus.Draft, BaselineProbability = 0.5m, DepartmentId = Guid.NewGuid() });
        await _context.SaveChangesAsync();

        var request = new GoalInfluenceRequest
        {
            GoalId = childGoalId,
            StrategyId = rootStrategyId,
            InfluenceType = "Positive",
            Strength = 0.8m,
            Confidence = 0.9m
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.GoalId.Should().Be(childGoalId);
        result.StrategyId.Should().Be(rootStrategyId);

        var dbInfluence = await _context.GoalInfluences.FindAsync(childGoalId);
        dbInfluence.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_ExistingInfluence_ShouldThrowInvalidGoalStateException()
    {
        // Arrange
        var rootGoalId = Guid.NewGuid();
        var rootStrategyId = Guid.NewGuid();
        var childGoalId = Guid.NewGuid();

        _context.Goals.Add(new Goal { Id = rootGoalId, Focus = "R", Object = "O", Magnitude = "M", Constraints = "C", Status = GoalStatus.Draft, BaselineProbability = 0.5m, DepartmentId = Guid.NewGuid() });
        _context.Strategies.Add(new Strategy { Id = rootStrategyId, Name = "S", GoalId = rootGoalId, Effectiveness = EffectivenessLevel.High, RefinementType = RefinementType.AND, Description = "" });
        _context.Goals.Add(new Goal { Id = childGoalId, Focus = "C", Object = "O", Magnitude = "M", Constraints = "C", Status = GoalStatus.Draft, BaselineProbability = 0.5m, DepartmentId = Guid.NewGuid() });
        _context.GoalInfluences.Add(new GoalInfluence { GoalId = childGoalId, StrategyId = rootStrategyId, InfluenceType = InfluenceType.Positive, Strength = 0.5m, Confidence = 0.5m, CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        var request = new GoalInfluenceRequest
        {
            GoalId = childGoalId,
            StrategyId = rootStrategyId, // Existing strategy
            InfluenceType = "Positive",
            Strength = 0.8m,
            Confidence = 0.9m
        };

        // Act
        Func<Task> act = async () => await _service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidGoalStateException>()
            .WithMessage("*already has an influence record*");
    }

    [Fact]
    public async Task CreateAsync_HierarchyCycle_ShouldThrowGoalHierarchyCycleException()
    {
        // Arrange - Setup a cycle: Goal A -> Strategy A -> Goal B -> Strategy B -> Goal A (Try to link Strat B to Goal A)
        var goalA = Guid.NewGuid();
        var stratA = Guid.NewGuid();
        var goalB = Guid.NewGuid();
        var stratB = Guid.NewGuid();

        _context.Goals.Add(new Goal { Id = goalA, Focus = "R", Object = "O", Magnitude = "M", Constraints = "C", Status = GoalStatus.Draft, BaselineProbability = 0.5m, DepartmentId = Guid.NewGuid() });
        _context.Strategies.Add(new Strategy { Id = stratA, Name = "SA", GoalId = goalA, Effectiveness = EffectivenessLevel.High, RefinementType = RefinementType.AND, Description = "" });
        
        _context.Goals.Add(new Goal { Id = goalB, Focus = "R", Object = "O", Magnitude = "M", Constraints = "C", Status = GoalStatus.Draft, BaselineProbability = 0.5m, DepartmentId = Guid.NewGuid() });
        // Goal B created from Strat A
        _context.GoalInfluences.Add(new GoalInfluence { GoalId = goalB, StrategyId = stratA, InfluenceType = InfluenceType.Positive, Strength = 1, Confidence = 1, CreatedAt = DateTime.UtcNow });
        
        // Strat B belongs to Goal B
        _context.Strategies.Add(new Strategy { Id = stratB, Name = "SB", GoalId = goalB, Effectiveness = EffectivenessLevel.High, RefinementType = RefinementType.AND, Description = "" });
        
        await _context.SaveChangesAsync();

        // Try to link Strat B to Goal A (this would create a cycle back to Goal A)
        var request = new GoalInfluenceRequest
        {
            GoalId = goalA,
            StrategyId = stratB,
            InfluenceType = "Positive",
            Strength = 1.0m,
            Confidence = 1.0m
        };

        // Act
        Func<Task> act = async () => await _service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<GoalHierarchyCycleException>();
    }
}
