using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OrchestrationService.Application.DTOs;
using OrchestrationService.Application.Interfaces.Clients;
using OrchestrationService.Application.Services;
using OrchestrationService.Domain.Entities;
using OrchestrationService.Domain.Enums;
using OrchestrationService.Domain.Exceptions;
using OrchestrationService.Infrastructure.Persistence;

namespace OrchestrationService.Tests.Application.Services;

public class WorkflowServiceTests : IDisposable
{
    private readonly OrchestrationDbContext _context;
    private readonly WorkflowService _service;
    private readonly Mock<IAuditClient> _auditMock;
    private readonly Mock<ICompensationHttpClient> _compensationMock;

    public WorkflowServiceTests()
    {
        var options = new DbContextOptionsBuilder<OrchestrationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new OrchestrationDbContext(options);
        _auditMock = new Mock<IAuditClient>();
        _compensationMock = new Mock<ICompensationHttpClient>();

        var logger = new Mock<ILogger<WorkflowService>>().Object;

        _service = new WorkflowService(_context, _auditMock.Object, _compensationMock.Object, logger);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    // ─── StartWorkflowAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task StartWorkflowAsync_ValidRequest_ShouldCreateWorkflow()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        var request = new StartWorkflowRequest { GoalId = goalId };

        // Act
        var result = await _service.StartWorkflowAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.GoalId.Should().Be(goalId);
        result.Status.Should().Be("InProgress");
        result.CurrentStep.Should().Be("GoalCreated");

        var dbWorkflow = await _context.SagaWorkflows.FirstOrDefaultAsync(w => w.GoalId == goalId);
        dbWorkflow.Should().NotBeNull();
    }

    [Fact]
    public async Task StartWorkflowAsync_DuplicateGoalId_ShouldThrowSagaAlreadyExistsException()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        _context.SagaWorkflows.Add(new SagaWorkflow
        {
            Id = Guid.NewGuid(),
            GoalId = goalId,
            Status = SagaStatus.InProgress,
            CurrentStep = WorkflowStep.GoalCreated,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var request = new StartWorkflowRequest { GoalId = goalId };

        // Act
        var act = async () => await _service.StartWorkflowAsync(request);

        // Assert
        await act.Should().ThrowAsync<SagaAlreadyExistsException>()
            .WithMessage($"*{goalId}*");
    }

    [Fact]
    public async Task StartWorkflowAsync_ShouldCallAuditLog()
    {
        // Arrange
        var request = new StartWorkflowRequest { GoalId = Guid.NewGuid() };

        // Act
        await _service.StartWorkflowAsync(request);

        // Assert — give fire-and-forget a moment
        await Task.Delay(50);
        _auditMock.Verify(a => a.LogAsync("WorkflowStarted", "SagaWorkflow",
            It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    // ─── GetWorkflowAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetWorkflowAsync_ExistingWorkflow_ShouldReturnWithSteps()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        var workflowId = Guid.NewGuid();
        var workflow = new SagaWorkflow
        {
            Id = workflowId,
            GoalId = goalId,
            Status = SagaStatus.InProgress,
            CurrentStep = WorkflowStep.PremisesAdded,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Steps =
            [
                new SagaStep
                {
                    Id = Guid.NewGuid(),
                    SagaWorkflowId = workflowId,
                    StepName = "GoalCreated",
                    Status = SagaStepStatus.Completed,
                    CompensationEndpoint = "api/Goal/123",
                    CompensationPayload = "{}",
                    ExecutedAt = DateTime.UtcNow
                }
            ]
        };

        _context.SagaWorkflows.Add(workflow);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetWorkflowAsync(goalId);

        // Assert
        result.Should().NotBeNull();
        result.GoalId.Should().Be(goalId);
        result.CurrentStep.Should().Be("PremisesAdded");
        result.Steps.Should().HaveCount(1);
        result.Steps[0].StepName.Should().Be("GoalCreated");
    }

    [Fact]
    public async Task GetWorkflowAsync_NonExistingGoalId_ShouldThrowSagaNotFoundException()
    {
        // Act
        var act = async () => await _service.GetWorkflowAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<SagaNotFoundException>();
    }

    // ─── RecordStepAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task RecordStepAsync_ValidRequest_ShouldAppendStep()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        var workflowId = Guid.NewGuid();
        _context.SagaWorkflows.Add(new SagaWorkflow
        {
            Id = workflowId,
            GoalId = goalId,
            Status = SagaStatus.InProgress,
            CurrentStep = WorkflowStep.GoalCreated,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var request = new RecordStepRequest
        {
            StepName = "PremisesAdded",
            CompensationEndpoint = "api/premises/by-goal/123",
            CompensationPayload = "{}"
        };

        // Act
        var result = await _service.RecordStepAsync(goalId, request);

        // Assert
        result.Should().NotBeNull();
        result.CurrentStep.Should().Be("PremisesAdded");
        result.Steps.Should().HaveCount(1);
        result.Steps[0].StepName.Should().Be("PremisesAdded");
        result.Steps[0].Status.Should().Be("Completed");
        result.Steps[0].CompensationEndpoint.Should().Be("api/premises/by-goal/123");
    }

    [Fact]
    public async Task RecordStepAsync_NonExistingWorkflow_ShouldThrowSagaNotFoundException()
    {
        // Arrange
        var request = new RecordStepRequest
        {
            StepName = "GoalCreated",
            CompensationEndpoint = "api/Goal/123",
            CompensationPayload = "{}"
        };

        // Act
        var act = async () => await _service.RecordStepAsync(Guid.NewGuid(), request);

        // Assert
        await act.Should().ThrowAsync<SagaNotFoundException>();
    }

    [Fact]
    public async Task RecordStepAsync_MultipleSteps_ShouldAccumulateAll()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        _context.SagaWorkflows.Add(new SagaWorkflow
        {
            Id = Guid.NewGuid(),
            GoalId = goalId,
            Status = SagaStatus.InProgress,
            CurrentStep = WorkflowStep.GoalCreated,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        await _service.RecordStepAsync(goalId, new RecordStepRequest
            { StepName = "GoalCreated", CompensationEndpoint = "api/Goal/1", CompensationPayload = "{}" });
        await _service.RecordStepAsync(goalId, new RecordStepRequest
            { StepName = "PremisesAdded", CompensationEndpoint = "api/premises/by-goal/1", CompensationPayload = "{}" });
        var result = await _service.RecordStepAsync(goalId, new RecordStepRequest
            { StepName = "AssessmentCreated", CompensationEndpoint = "api/assessments/by-goal/1", CompensationPayload = "{}" });

        // Assert
        result.Steps.Should().HaveCount(3);
        result.CurrentStep.Should().Be("AssessmentCreated");
    }

    // ─── CancelWorkflowAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task CancelWorkflowAsync_ShouldCompensateAllCompletedStepsInReverseOrder()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        var workflowId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var step1 = new SagaStep
        {
            Id = Guid.NewGuid(), SagaWorkflowId = workflowId, StepName = "GoalCreated",
            Status = SagaStepStatus.Completed, CompensationEndpoint = "api/Goal/1",
            CompensationPayload = "{}", ExecutedAt = now.AddMinutes(-10)
        };
        var step2 = new SagaStep
        {
            Id = Guid.NewGuid(), SagaWorkflowId = workflowId, StepName = "PremisesAdded",
            Status = SagaStepStatus.Completed, CompensationEndpoint = "api/premises/by-goal/1",
            CompensationPayload = "{}", ExecutedAt = now.AddMinutes(-5)
        };

        _context.SagaWorkflows.Add(new SagaWorkflow
        {
            Id = workflowId, GoalId = goalId, Status = SagaStatus.InProgress,
            CurrentStep = WorkflowStep.PremisesAdded, CreatedAt = now, UpdatedAt = now,
            Steps = [step1, step2]
        });
        await _context.SaveChangesAsync();

        var callOrder = new List<string>();
        _compensationMock
            .Setup(c => c.CallAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string ep, string _) =>
            {
                callOrder.Add(ep);
                return true;
            });

        // Act
        var result = await _service.CancelWorkflowAsync(goalId);

        // Assert
        result.Status.Should().Be("Compensated");

        // Verify reverse order (step2 first, then step1)
        callOrder.Should().ContainInOrder("api/premises/by-goal/1", "api/Goal/1");

        var dbWorkflow = await _context.SagaWorkflows
            .Include(w => w.Steps)
            .FirstAsync(w => w.GoalId == goalId);

        dbWorkflow.Status.Should().Be(SagaStatus.Compensated);
        dbWorkflow.Steps.Should().AllSatisfy(s => s.Status.Should().Be(SagaStepStatus.Compensated));
        dbWorkflow.Steps.Should().AllSatisfy(s => s.CompensatedAt.Should().NotBeNull());
    }

    [Fact]
    public async Task CancelWorkflowAsync_FailedCompensationCall_ShouldContinueAndMarkCompensated()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        var workflowId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        _context.SagaWorkflows.Add(new SagaWorkflow
        {
            Id = workflowId, GoalId = goalId, Status = SagaStatus.InProgress,
            CurrentStep = WorkflowStep.GoalCreated, CreatedAt = now, UpdatedAt = now,
            Steps =
            [
                new SagaStep
                {
                    Id = Guid.NewGuid(), SagaWorkflowId = workflowId, StepName = "GoalCreated",
                    Status = SagaStepStatus.Completed, CompensationEndpoint = "api/Goal/1",
                    CompensationPayload = "{}", ExecutedAt = now
                }
            ]
        });
        await _context.SaveChangesAsync();

        // Compensation returns false (failure)
        _compensationMock
            .Setup(c => c.CallAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act — should NOT throw even though compensation failed
        var result = await _service.CancelWorkflowAsync(goalId);

        // Assert — workflow is still marked Compensated
        result.Status.Should().Be("Compensated");
    }

    [Fact]
    public async Task CancelWorkflowAsync_AlreadyCompensated_ShouldThrowSagaAlreadyCompensatedException()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        _context.SagaWorkflows.Add(new SagaWorkflow
        {
            Id = Guid.NewGuid(), GoalId = goalId, Status = SagaStatus.Compensated,
            CurrentStep = WorkflowStep.GoalCreated, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var act = async () => await _service.CancelWorkflowAsync(goalId);

        // Assert
        await act.Should().ThrowAsync<SagaAlreadyCompensatedException>()
            .WithMessage($"*{goalId}*");
    }

    [Fact]
    public async Task CancelWorkflowAsync_NonExistingWorkflow_ShouldThrowSagaNotFoundException()
    {
        // Act
        var act = async () => await _service.CancelWorkflowAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<SagaNotFoundException>();
    }

    [Fact]
    public async Task CancelWorkflowAsync_ShouldCallAuditLog()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        _context.SagaWorkflows.Add(new SagaWorkflow
        {
            Id = Guid.NewGuid(), GoalId = goalId, Status = SagaStatus.InProgress,
            CurrentStep = WorkflowStep.GoalCreated, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        await _service.CancelWorkflowAsync(goalId);
        await Task.Delay(50);

        // Assert
        _auditMock.Verify(a => a.LogAsync("WorkflowCompensated", "SagaWorkflow",
            It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    // ─── GetStepsAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStepsAsync_ExistingWorkflow_ShouldReturnStepsOrderedByExecutedAt()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        var workflowId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        _context.SagaWorkflows.Add(new SagaWorkflow
        {
            Id = workflowId, GoalId = goalId, Status = SagaStatus.InProgress,
            CurrentStep = WorkflowStep.PremisesAdded, CreatedAt = now, UpdatedAt = now,
            Steps =
            [
                new SagaStep
                {
                    Id = Guid.NewGuid(), SagaWorkflowId = workflowId, StepName = "PremisesAdded",
                    Status = SagaStepStatus.Completed, CompensationEndpoint = "api/premises/by-goal/1",
                    CompensationPayload = "{}", ExecutedAt = now.AddMinutes(-1)
                },
                new SagaStep
                {
                    Id = Guid.NewGuid(), SagaWorkflowId = workflowId, StepName = "GoalCreated",
                    Status = SagaStepStatus.Completed, CompensationEndpoint = "api/Goal/1",
                    CompensationPayload = "{}", ExecutedAt = now.AddMinutes(-5)
                }
            ]
        });
        await _context.SaveChangesAsync();

        // Act
        var result = (await _service.GetStepsAsync(goalId)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].StepName.Should().Be("GoalCreated");   // earlier ExecutedAt first
        result[1].StepName.Should().Be("PremisesAdded");
    }

    [Fact]
    public async Task GetStepsAsync_NonExistingWorkflow_ShouldThrowSagaNotFoundException()
    {
        // Act
        var act = async () => await _service.GetStepsAsync(Guid.NewGuid());

        // Assert
        await act.Should().ThrowAsync<SagaNotFoundException>();
    }
}
