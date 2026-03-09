using FluentAssertions;
using GoalService.Application.DTOs;
using GoalService.Application.DTOs.External;
using Shared.Contracts;
using GoalService.Application.Services;
using GoalService.Domain.Entities;
using GoalService.Domain.Enums;
using GoalService.Domain.Exceptions;
using GoalService.Infrastructure.Persistence;
using MassTransit;
using Shared.Contracts.Messages;
using GoalService.Application.Interfaces.Clients;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GoalService.Tests.Application.Services;

public class GoalServiceImplTests : IDisposable
{
    private readonly GoalDbContext _context;
    private readonly GoalServiceImpl _service;
    private readonly Mock<IPremiseClient> _premiseClientMock;
    private readonly Mock<IAssessmentClient> _assessmentClientMock;
    private readonly Mock<IQgmGoalClient> _qgmGoalClientMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;

    public GoalServiceImplTests()
    {
        var options = new DbContextOptionsBuilder<GoalDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new GoalDbContext(options);
        _premiseClientMock = new Mock<IPremiseClient>();
        _assessmentClientMock = new Mock<IAssessmentClient>();
        _qgmGoalClientMock = new Mock<IQgmGoalClient>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();

        _service = new GoalServiceImpl(
            _context, 
            _premiseClientMock.Object, 
            _assessmentClientMock.Object, 
            _qgmGoalClientMock.Object,
            _publishEndpointMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task GetAllPaginatedAsync_ShouldReturnPaginatedGoals_WithRelations()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        
        var goal = new Goal
        {
            Id = goalId,
            Focus = "Focus",
            Object = "Object",
            ActiveFrom = DateTime.UtcNow,
            ActiveTo = DateTime.UtcNow.AddYears(1),
            Magnitude = "Mag",
            Constraints = "Con",
            Status = GoalStatus.Active,
            BaselineProbability = 0.5m,
            DepartmentId = departmentId
        };
        
        var strategy = new Strategy
        {
            Id = Guid.NewGuid(),
            Name = "Strat",
            Description = "Desc",
            Effectiveness = EffectivenessLevel.High,
            RefinementType = RefinementType.AND,
            GoalId = goalId
        };

        _context.Goals.Add(goal);
        _context.Strategies.Add(strategy);
        await _context.SaveChangesAsync();

        // Act
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };
        var result = await _service.GetAllPaginatedAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Total.Should().Be(1);
        
        var dto = result.Items.First();
        dto.Id.Should().Be(goalId);
        dto.Strategies.Should().HaveCount(1);
        dto.Strategies.First().Name.Should().Be("Strat");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingGoal_ShouldReturnGoalResponse()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        _context.Goals.Add(new Goal
        {
            Id = goalId,
            Focus = "Focus",
            Object = "Object",
            ActiveFrom = DateTime.UtcNow,
            ActiveTo = DateTime.UtcNow.AddYears(1),
            Magnitude = "Mag",
            Constraints = "Con",
            Status = GoalStatus.Active,
            BaselineProbability = 0.5m,
            DepartmentId = Guid.NewGuid()
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(goalId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(goalId);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingGoal_ShouldReturnNull()
    {
        // Act
        var result = await _service.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ShouldCreateGoal()
    {
        // Arrange
        var request = new GoalRequest
        {
            Focus = "New Focus",
            Object = "New Object",
            ActiveFrom = DateTime.UtcNow,
            ActiveTo = DateTime.UtcNow.AddYears(1),
            Magnitude = "10%",
            Constraints = "None",
            Status = "Draft",
            BaselineProbability = 0.8m,
            DepartmentId = Guid.NewGuid()
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Focus.Should().Be("New Focus");
        result.Status.Should().Be("Draft");

        var dbGoal = await _context.Goals.FindAsync(result.Id);
        dbGoal.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_ExistingGoal_ShouldUpdateFields()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        _context.Goals.Add(new Goal
        {
            Id = goalId,
            Focus = "Old",
            Object = "Object",
            ActiveFrom = DateTime.UtcNow,
            ActiveTo = DateTime.UtcNow.AddYears(1),
            Magnitude = "Mag",
            Constraints = "Con",
            Status = GoalStatus.Draft,
            BaselineProbability = 0.5m,
            DepartmentId = Guid.NewGuid()
        });
        await _context.SaveChangesAsync();

        var updateRequest = new GoalRequest
        {
            Focus = "New",
            Object = "Object",
            ActiveFrom = DateTime.UtcNow,
            ActiveTo = DateTime.UtcNow.AddYears(1),
            Magnitude = "Mag",
            Constraints = "Con",
            Status = "Active",
            BaselineProbability = 0.6m,
            DepartmentId = Guid.NewGuid()
        };

        // Act
        var result = await _service.UpdateAsync(goalId, updateRequest);

        // Assert
        result.Should().NotBeNull();
        var dbGoal = await _context.Goals.FindAsync(goalId);
        dbGoal!.Focus.Should().Be("New");
        dbGoal.Status.Should().Be(GoalStatus.Active);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingGoal_ShouldReturnNull()
    {
        // Arrange
        var updateRequest = new GoalRequest
        {
            Focus = "New",
            Object = "Object",
            ActiveFrom = DateTime.UtcNow,
            ActiveTo = DateTime.UtcNow.AddYears(1),
            Magnitude = "Mag",
            Constraints = "Con",
            Status = "Active",
            BaselineProbability = 0.6m,
            DepartmentId = Guid.NewGuid()
        };

        // Act
        var result = await _service.UpdateAsync(Guid.NewGuid(), updateRequest);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ExistingGoal_ShouldRemoveGoal()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        _context.Goals.Add(new Goal
        {
            Id = goalId,
            Focus = "Old",
            Object = "Object",
            ActiveFrom = DateTime.UtcNow,
            ActiveTo = DateTime.UtcNow.AddYears(1),
            Magnitude = "Mag",
            Constraints = "Con",
            Status = GoalStatus.Draft,
            BaselineProbability = 0.5m,
            DepartmentId = Guid.NewGuid()
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteAsync(goalId);

        // Assert
        result.Should().BeTrue();
        var dbGoal = await _context.Goals.FindAsync(goalId);
        dbGoal.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingGoal_ShouldReturnFalse()
    {
        // Act
        var result = await _service.DeleteAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    // ---- ActivateAsync tests ----

    private Goal CreateDraftGoal(Guid? id = null) => new Goal
    {
        Id = id ?? Guid.NewGuid(),
        Focus = "Focus",
        Object = "Object",
        ActiveFrom = DateTime.UtcNow,
        ActiveTo = DateTime.UtcNow.AddYears(1),
        Magnitude = "Mag",
        Constraints = "Con",
        Status = GoalStatus.Draft,
        BaselineProbability = 0.5m,
        DepartmentId = Guid.NewGuid()
    };

    [Fact]
    public async Task ActivateAsync_GoalNotFound_ReturnsNull()
    {
        var result = await _service.ActivateAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    [Fact]
    public async Task ActivateAsync_NotDraft_ThrowsInvalidGoalStateException()
    {
        var goal = CreateDraftGoal();
        goal.Status = GoalStatus.Active;
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        await _service.Invoking(s => s.ActivateAsync(goal.Id))
            .Should().ThrowAsync<InvalidGoalStateException>();
    }

    [Fact]
    public async Task ActivateAsync_NoActiveStrategy_ThrowsGoalActivationException()
    {
        var goal = CreateDraftGoal();
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        _assessmentClientMock.Setup(x => x.GetAssessmentsForGoalAsync(goal.Id))
            .ReturnsAsync(new[] { new AssessmentDto { GoalId = goal.Id, State = "Completed" } });
        _qgmGoalClientMock.Setup(x => x.GetQgmGoalsForGoalAsync(goal.Id))
            .ReturnsAsync(new[] { new QgmGoalDto { GoalId = goal.Id } });

        var ex = await _service.Invoking(s => s.ActivateAsync(goal.Id))
            .Should().ThrowAsync<GoalActivationException>();
        ex.Which.Blockers.Should().Contain(b => b.Contains("strategy"));
    }

    [Fact]
    public async Task ActivateAsync_NoAssessment_ThrowsGoalActivationException()
    {
        var goal = CreateDraftGoal();
        _context.Goals.Add(goal);
        _context.Strategies.Add(new Strategy { Id = Guid.NewGuid(), Name = "S", Description = "D",
            Effectiveness = EffectivenessLevel.High, RefinementType = RefinementType.AND,
            GoalId = goal.Id, IsActive = true });
        await _context.SaveChangesAsync();

        _assessmentClientMock.Setup(x => x.GetAssessmentsForGoalAsync(goal.Id))
            .ReturnsAsync(Enumerable.Empty<AssessmentDto>());
        _qgmGoalClientMock.Setup(x => x.GetQgmGoalsForGoalAsync(goal.Id))
            .ReturnsAsync(new[] { new QgmGoalDto { GoalId = goal.Id } });

        var ex = await _service.Invoking(s => s.ActivateAsync(goal.Id))
            .Should().ThrowAsync<GoalActivationException>();
        ex.Which.Blockers.Should().Contain(b => b.Contains("assessment"));
    }

    [Fact]
    public async Task ActivateAsync_AssessmentNotCompleted_ThrowsGoalActivationException()
    {
        var goal = CreateDraftGoal();
        _context.Goals.Add(goal);
        _context.Strategies.Add(new Strategy { Id = Guid.NewGuid(), Name = "S", Description = "D",
            Effectiveness = EffectivenessLevel.High, RefinementType = RefinementType.AND,
            GoalId = goal.Id, IsActive = true });
        await _context.SaveChangesAsync();

        _assessmentClientMock.Setup(x => x.GetAssessmentsForGoalAsync(goal.Id))
            .ReturnsAsync(new[] { new AssessmentDto { GoalId = goal.Id, State = "Draft" } });
        _qgmGoalClientMock.Setup(x => x.GetQgmGoalsForGoalAsync(goal.Id))
            .ReturnsAsync(new[] { new QgmGoalDto { GoalId = goal.Id } });

        var ex = await _service.Invoking(s => s.ActivateAsync(goal.Id))
            .Should().ThrowAsync<GoalActivationException>();
        ex.Which.Blockers.Should().Contain(b => b.Contains("Completed"));
    }

    [Fact]
    public async Task ActivateAsync_NoGqmGoal_ThrowsGoalActivationException()
    {
        var goal = CreateDraftGoal();
        _context.Goals.Add(goal);
        _context.Strategies.Add(new Strategy { Id = Guid.NewGuid(), Name = "S", Description = "D",
            Effectiveness = EffectivenessLevel.High, RefinementType = RefinementType.AND,
            GoalId = goal.Id, IsActive = true });
        await _context.SaveChangesAsync();

        _assessmentClientMock.Setup(x => x.GetAssessmentsForGoalAsync(goal.Id))
            .ReturnsAsync(new[] { new AssessmentDto { GoalId = goal.Id, State = "Completed" } });
        _qgmGoalClientMock.Setup(x => x.GetQgmGoalsForGoalAsync(goal.Id))
            .ReturnsAsync(Enumerable.Empty<QgmGoalDto>());

        var ex = await _service.Invoking(s => s.ActivateAsync(goal.Id))
            .Should().ThrowAsync<GoalActivationException>();
        ex.Which.Blockers.Should().Contain(b => b.Contains("GQM"));
    }

    [Fact]
    public async Task ActivateAsync_MultipleBlockers_ReturnsAllBlockers()
    {
        var goal = CreateDraftGoal();
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        _assessmentClientMock.Setup(x => x.GetAssessmentsForGoalAsync(goal.Id))
            .ReturnsAsync(Enumerable.Empty<AssessmentDto>());
        _qgmGoalClientMock.Setup(x => x.GetQgmGoalsForGoalAsync(goal.Id))
            .ReturnsAsync(Enumerable.Empty<QgmGoalDto>());

        var ex = await _service.Invoking(s => s.ActivateAsync(goal.Id))
            .Should().ThrowAsync<GoalActivationException>();
        ex.Which.Blockers.Should().HaveCount(3);
    }

    [Fact]
    public async Task ActivateAsync_AllPrerequisitesMet_ReturnsActiveGoal()
    {
        var goal = CreateDraftGoal();
        _context.Goals.Add(goal);
        _context.Strategies.Add(new Strategy { Id = Guid.NewGuid(), Name = "S", Description = "D",
            Effectiveness = EffectivenessLevel.High, RefinementType = RefinementType.AND,
            GoalId = goal.Id, IsActive = true });
        await _context.SaveChangesAsync();

        _assessmentClientMock.Setup(x => x.GetAssessmentsForGoalAsync(goal.Id))
            .ReturnsAsync(new[] { new AssessmentDto { GoalId = goal.Id, State = "Completed" } });
        _qgmGoalClientMock.Setup(x => x.GetQgmGoalsForGoalAsync(goal.Id))
            .ReturnsAsync(new[] { new QgmGoalDto { GoalId = goal.Id } });

        var result = await _service.ActivateAsync(goal.Id);

        result.Should().NotBeNull();
        result!.Status.Should().Be("Active");
        var dbGoal = await _context.Goals.FindAsync(goal.Id);
        dbGoal!.Status.Should().Be(GoalStatus.Active);
    }
}
