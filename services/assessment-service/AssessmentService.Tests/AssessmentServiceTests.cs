using AssessmentService.Application.DTOs;
using AssessmentService.Application.Interfaces.Clients;
using AssessmentService.Application.Services;
using AssessmentService.Domain.Enums;
using AssessmentService.Domain.Exceptions;
using AssessmentService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AssessmentService.Tests;

public class AssessmentServiceTests : IDisposable
{
    private readonly AssessmentDbContext _dbContext;
    private readonly AssessmentServiceImpl _service;
    private readonly Mock<IOrchestrationClient> _orchestrationClientMock;

    public AssessmentServiceTests()
    {
        var options = new DbContextOptionsBuilder<AssessmentDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AssessmentDbContext(options);
        _orchestrationClientMock = new Mock<IOrchestrationClient>();
        _service = new AssessmentServiceImpl(_dbContext, _orchestrationClientMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedAssessment()
    {
        var request = new CreateAssessmentRequest(
            GoalId: Guid.NewGuid(),
            Probability: 0.85m,
            State: AssessmentState.Draft,
            Method: AssessmentMethod.Expert,
            Notes: "Initial assessment"
        );

        var result = await _service.CreateAsync(request);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(request.GoalId, result.GoalId);
        Assert.Equal(request.Probability, result.Probability);
        Assert.Equal(AssessmentState.Draft, result.State);
        Assert.Equal(AssessmentMethod.Expert, result.Method);
        Assert.Equal("Initial assessment", result.Notes);
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistToDatabase()
    {
        var request = new CreateAssessmentRequest(
            GoalId: Guid.NewGuid(),
            Probability: 0.50m,
            State: AssessmentState.InProgress,
            Method: AssessmentMethod.DataDriven,
            Notes: "Persisted assessment"
        );

        var result = await _service.CreateAsync(request);

        var persisted = await _dbContext.GoalProbabilityAssessments.FindAsync(result.Id);
        Assert.NotNull(persisted);
        Assert.Equal(request.GoalId, persisted.GoalId);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnAssessment_WhenExists()
    {
        var request = new CreateAssessmentRequest(
            GoalId: Guid.NewGuid(),
            Probability: 0.75m,
            State: AssessmentState.Completed,
            Method: AssessmentMethod.Hybrid,
            Notes: "Completed assessment"
        );
        var created = await _service.CreateAsync(request);

        var result = await _service.GetByIdAsync(created.Id);

        Assert.Equal(created.Id, result.Id);
        Assert.Equal(created.GoalId, result.GoalId);
        Assert.Equal(AssessmentState.Completed, result.State);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowAssessmentNotFoundException_WhenNotExists()
    {
        var nonExistentId = Guid.NewGuid();

        await Assert.ThrowsAsync<AssessmentNotFoundException>(
            () => _service.GetByIdAsync(nonExistentId));
    }

    [Fact]
    public async Task GetByGoalIdAsync_ShouldReturnAssessment_WhenExists()
    {
        var goalId = Guid.NewGuid();
        var request = new CreateAssessmentRequest(
            GoalId: goalId,
            Probability: 0.60m,
            State: AssessmentState.Draft,
            Method: AssessmentMethod.Expert,
            Notes: "Goal-specific assessment"
        );
        await _service.CreateAsync(request);

        var result = await _service.GetByGoalIdAsync(goalId);

        Assert.NotNull(result);
        Assert.Equal(goalId, result.GoalId);
    }

    [Fact]
    public async Task GetByGoalIdAsync_ShouldThrow_WhenNotExists()
    {
        await Assert.ThrowsAsync<AssessmentByGoalNotFoundException>(
            () => _service.GetByGoalIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateAllFields()
    {
        var created = await _service.CreateAsync(new CreateAssessmentRequest(
            GoalId: Guid.NewGuid(),
            Probability: 0.30m,
            State: AssessmentState.Draft,
            Method: AssessmentMethod.Expert,
            Notes: "Before update"
        ));

        var updateRequest = new UpdateAssessmentRequest(
            Probability: 0.95m,
            State: AssessmentState.Completed,
            Method: AssessmentMethod.DataDriven,
            Notes: "After update"
        );

        var result = await _service.UpdateAsync(created.Id, updateRequest);

        Assert.Equal(created.Id, result.Id);
        Assert.Equal(0.95m, result.Probability);
        Assert.Equal(AssessmentState.Completed, result.State);
        Assert.Equal(AssessmentMethod.DataDriven, result.Method);
        Assert.Equal("After update", result.Notes);
    }

    [Fact]
    public async Task UpdateAsync_ShouldNotChangeGoalId()
    {
        var originalGoalId = Guid.NewGuid();
        var created = await _service.CreateAsync(new CreateAssessmentRequest(
            GoalId: originalGoalId,
            Probability: 0.40m,
            State: AssessmentState.Draft,
            Method: AssessmentMethod.Expert,
            Notes: "Original"
        ));

        var updateRequest = new UpdateAssessmentRequest(
            Probability: 0.80m,
            State: AssessmentState.InProgress,
            Method: AssessmentMethod.Hybrid,
            Notes: "Updated"
        );

        var result = await _service.UpdateAsync(created.Id, updateRequest);

        Assert.Equal(originalGoalId, result.GoalId);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowAssessmentNotFoundException_WhenNotExists()
    {
        var updateRequest = new UpdateAssessmentRequest(
            Probability: 0.50m,
            State: AssessmentState.Draft,
            Method: AssessmentMethod.Expert,
            Notes: "Does not matter"
        );

        await Assert.ThrowsAsync<AssessmentNotFoundException>(
            () => _service.UpdateAsync(Guid.NewGuid(), updateRequest));
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveAssessment_WhenExists()
    {
        var created = await _service.CreateAsync(new CreateAssessmentRequest(
            GoalId: Guid.NewGuid(),
            Probability: 0.70m,
            State: AssessmentState.Archived,
            Method: AssessmentMethod.Hybrid,
            Notes: "To be deleted"
        ));

        await _service.DeleteAsync(created.Id);

        var persisted = await _dbContext.GoalProbabilityAssessments.FindAsync(created.Id);
        Assert.Null(persisted);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowAssessmentNotFoundException_WhenNotExists()
    {
        await Assert.ThrowsAsync<AssessmentNotFoundException>(
            () => _service.DeleteAsync(Guid.NewGuid()));
    }
}
