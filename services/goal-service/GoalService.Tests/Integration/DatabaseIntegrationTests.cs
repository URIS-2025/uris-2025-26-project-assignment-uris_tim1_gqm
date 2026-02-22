using FluentAssertions;
using GoalService.Domain.Entities;
using GoalService.Domain.Enums;
using GoalService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;
using Xunit;

namespace GoalService.Tests.Integration;

public class DatabaseIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer;
    private GoalDbContext _context = null!;

    public DatabaseIntegrationTests()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("test_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        var options = new DbContextOptionsBuilder<GoalDbContext>()
            .UseNpgsql(_postgreSqlContainer.GetConnectionString())
            .Options;

        _context = new GoalDbContext(options);
        await _context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgreSqlContainer.DisposeAsync();
    }

    [Fact]
    public async Task CanInsertAndRetrieve_GoalWithStrategiesAndInfluences()
    {
        // Arrange
        var rootGoalId = Guid.NewGuid();
        var strategyId = Guid.NewGuid();
        var childGoalId = Guid.NewGuid();

        var rootGoal = new Goal
        {
            Id = rootGoalId,
            Focus = "Root Focus",
            Object = "Root Object",
            ActiveFrom = DateTime.UtcNow,
            ActiveTo = DateTime.UtcNow.AddYears(1),
            Magnitude = "Mag",
            Constraints = "Con",
            Status = GoalStatus.Active,
            BaselineProbability = 0.5m,
            DepartmentId = Guid.NewGuid()
        };

        var strategy = new Strategy
        {
            Id = strategyId,
            Name = "Strategy 1",
            Description = "Desc",
            Effectiveness = EffectivenessLevel.High,
            RefinementType = RefinementType.AND,
            GoalId = rootGoalId
        };

        var childGoal = new Goal
        {
            Id = childGoalId,
            Focus = "Child Focus",
            Object = "Child Object",
            ActiveFrom = DateTime.UtcNow,
            ActiveTo = DateTime.UtcNow.AddYears(1),
            Magnitude = "Mag",
            Constraints = "Con",
            Status = GoalStatus.Draft,
            BaselineProbability = 0.5m,
            DepartmentId = Guid.NewGuid()
        };

        var influence = new GoalInfluence
        {
            GoalId = childGoalId,
            StrategyId = strategyId,
            InfluenceType = InfluenceType.Positive,
            Strength = 1.0m,
            Confidence = 0.9m,
            CreatedAt = DateTime.UtcNow,
            Notes = "Testing integration"
        };

        // Act - Insert
        _context.Goals.Add(rootGoal);
        _context.Strategies.Add(strategy);
        _context.Goals.Add(childGoal);
        _context.GoalInfluences.Add(influence);
        await _context.SaveChangesAsync();

        // Assert - Retrieve and verify EF Core eager loading
        var retrievedGoal = await _context.Goals
            .Include(g => g.Strategies)
                .ThenInclude(s => s.GoalInfluences)
            .FirstOrDefaultAsync(g => g.Id == rootGoalId);

        retrievedGoal.Should().NotBeNull();
        retrievedGoal!.Strategies.Should().HaveCount(1);
        retrievedGoal.Strategies.First().GoalInfluences.Should().HaveCount(1);
        retrievedGoal.Strategies.First().GoalInfluences.First().GoalId.Should().Be(childGoalId);
    }
    
    [Fact]
    public async Task GoalDeletion_ShouldCascadeDelete_StrategiesAndInfluences()
    {
        // Arrange
        var rootGoalId = Guid.NewGuid();
        var strategyId = Guid.NewGuid();
        var childGoalId = Guid.NewGuid();

        _context.Goals.Add(new Goal { Id = rootGoalId, Focus = "R", Object = "O", Magnitude = "M", Constraints = "C", Status = GoalStatus.Active, BaselineProbability = 0.5m, DepartmentId = Guid.NewGuid() });
        _context.Strategies.Add(new Strategy { Id = strategyId, Name = "S", Description = "D", Effectiveness = EffectivenessLevel.High, RefinementType = RefinementType.AND, GoalId = rootGoalId });
        _context.Goals.Add(new Goal { Id = childGoalId, Focus = "C", Object = "O", Magnitude = "M", Constraints = "C", Status = GoalStatus.Draft, BaselineProbability = 0.5m, DepartmentId = Guid.NewGuid() });
        _context.GoalInfluences.Add(new GoalInfluence { GoalId = childGoalId, StrategyId = strategyId, InfluenceType = InfluenceType.Positive, Strength = 1.0m, Confidence = 0.9m, CreatedAt = DateTime.UtcNow });
        await _context.SaveChangesAsync();

        // Act
        var rootGoal = await _context.Goals.FindAsync(rootGoalId);
        _context.Goals.Remove(rootGoal!);
        await _context.SaveChangesAsync();

        // Assert
        var strategiesExist = await _context.Strategies.AnyAsync(s => s.GoalId == rootGoalId);
        strategiesExist.Should().BeFalse();

        var influencesExist = await _context.GoalInfluences.AnyAsync(gi => gi.StrategyId == strategyId);
        influencesExist.Should().BeFalse();
        
        // Child goal should still exist (cascade delete only removes the link/influence, not the target goal itself unless configured differently, but in our domain, deleting a strategy DELETES the child goal? Wait, no, cascade delete deletes GoalInfluence, but the child Goal remains as a detached Goal).
        var childGoalExists = await _context.Goals.AnyAsync(g => g.Id == childGoalId);
        childGoalExists.Should().BeTrue();
    }
}
