using AssessmentService.Application.DTOs;
using AssessmentService.Application.Interfaces;
using AssessmentService.Domain.Entities;
using AssessmentService.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AssessmentService.Application.Services;

public class AssessmentServiceImpl : IAssessmentService
{
    private readonly IAssessmentDbContext _dbContext;

    public AssessmentServiceImpl(IAssessmentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AssessmentResponse> CreateAsync(CreateAssessmentRequest request)
    {
        var exists = await _dbContext.GoalProbabilityAssessments
            .AsNoTracking()
            .AnyAsync(a => a.GoalId == request.GoalId);

        if (exists)
            throw new AssessmentAlreadyExistsException(request.GoalId);

        var assessment = new GoalProbabilityAssessment
        {
            Id = Guid.NewGuid(),
            GoalId = request.GoalId,
            Probability = request.Probability,
            State = request.State,
            Method = request.Method,
            Notes = request.Notes
        };

        await _dbContext.GoalProbabilityAssessments.AddAsync(assessment);
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new AssessmentAlreadyExistsException(request.GoalId);
        }

        return MapToResponse(assessment);
    }

    public async Task<AssessmentResponse> GetByIdAsync(Guid id)
    {
        var assessment = await _dbContext.GoalProbabilityAssessments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (assessment is null)
            throw new AssessmentNotFoundException(id);

        return MapToResponse(assessment);
    }

    public async Task<AssessmentResponse?> GetByGoalIdAsync(Guid goalId)
    {
        var assessment = await _dbContext.GoalProbabilityAssessments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.GoalId == goalId);
        
        if (assessment is null)
            throw new AssessmentByGoalNotFoundException(goalId);

        return MapToResponse(assessment);
    }

    public async Task<AssessmentResponse> UpdateAsync(Guid id, UpdateAssessmentRequest request)
    {
        var assessment = await _dbContext.GoalProbabilityAssessments
            .FirstOrDefaultAsync(a => a.Id == id);

        if (assessment is null)
            throw new AssessmentNotFoundException(id);

        assessment.Probability = request.Probability;
        assessment.State = request.State;
        assessment.Method = request.Method;
        assessment.Notes = request.Notes;

        await _dbContext.SaveChangesAsync();

        return MapToResponse(assessment);
    }

    public async Task DeleteAsync(Guid id)
    {
        var assessment = await _dbContext.GoalProbabilityAssessments
            .FirstOrDefaultAsync(a => a.Id == id);

        if (assessment is null)
            throw new AssessmentNotFoundException(id);

        _dbContext.GoalProbabilityAssessments.Remove(assessment);
        await _dbContext.SaveChangesAsync();
    }

    private static AssessmentResponse MapToResponse(GoalProbabilityAssessment assessment)
    {
        return new AssessmentResponse(
            assessment.Id,
            assessment.GoalId,
            assessment.Probability,
            assessment.State,
            assessment.Method,
            assessment.Notes
        );
    }
}
