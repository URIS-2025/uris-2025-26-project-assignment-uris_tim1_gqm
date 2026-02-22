using FluentAssertions;
using GoalService.Application.DTOs;
using GoalService.Infrastructure.Services;
using GoalService.Domain.Entities;
using GoalService.Domain.Enums;
using GoalService.Domain.Exceptions;
using GoalService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GoalService.Tests.Application.Services;

public class StrategyServiceImplTests : IDisposable
{
    private readonly GoalDbContext _context;
    private readonly StrategyServiceImpl _service;

    public StrategyServiceImplTests()
    {
        var options = new DbContextOptionsBuilder<GoalDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new GoalDbContext(options);
        _service = new StrategyServiceImpl(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetByGoalIdAsync_ShouldReturnStrategiesForGoal()
    {
        // Arrange
        var targetGoalId = Guid.NewGuid();
        var strategy1 = new Strategy { Id = Guid.NewGuid(), Name = "S1", GoalId = targetGoalId, Effectiveness = EffectivenessLevel.High, RefinementType = RefinementType.AND, Description = "" };
        var strategy2 = new Strategy { Id = Guid.NewGuid(), Name = "S2", GoalId = targetGoalId, Effectiveness = EffectivenessLevel.Low, RefinementType = RefinementType.OR, Description = "" };
        var otherStrategy = new Strategy { Id = Guid.NewGuid(), Name = "S3", GoalId = Guid.NewGuid(), Effectiveness = EffectivenessLevel.Medium, RefinementType = RefinementType.AND, Description = "" };

        _context.Strategies.AddRange(strategy1, strategy2, otherStrategy);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByGoalIdAsync(targetGoalId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(s => s.Id == strategy1.Id);
        result.Should().Contain(s => s.Id == strategy2.Id);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ShouldCreateStrategy()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        _context.Goals.Add(new Goal { Id = goalId, Focus = "F", Object = "O", Magnitude = "M", Constraints = "C", Status = GoalStatus.Draft, BaselineProbability = 0.5m, DepartmentId = Guid.NewGuid() });
        await _context.SaveChangesAsync();

        var request = new StrategyRequest
        {
            Name = "New Strat",
            Description = "Desc",
            Effectiveness = "High",
            RefinementType = "AND",
            GoalId = goalId
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Strat");
        result.GoalId.Should().Be(goalId);

        var dbStrategy = await _context.Strategies.FindAsync(result.Id);
        dbStrategy.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_NonExistingGoal_ShouldThrowGoalNotFoundException()
    {
        // Arrange
        var request = new StrategyRequest
        {
            Name = "New Strat",
            Description = "Desc",
            Effectiveness = "High",
            RefinementType = "AND",
            GoalId = Guid.NewGuid() // Goal does not exist
        };

        // Act
        Func<Task> act = async () => await _service.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<GoalNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_ExistingStrategy_ShouldReturnTrue()
    {
        // Arrange
        var strategyId = Guid.NewGuid();
        _context.Strategies.Add(new Strategy { Id = strategyId, Name = "S", GoalId = Guid.NewGuid(), Effectiveness = EffectivenessLevel.High, RefinementType = RefinementType.AND, Description = "" });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteAsync(strategyId);

        // Assert
        result.Should().BeTrue();
        var dbStrategy = await _context.Strategies.FindAsync(strategyId);
        dbStrategy.Should().BeNull();
    }
}
