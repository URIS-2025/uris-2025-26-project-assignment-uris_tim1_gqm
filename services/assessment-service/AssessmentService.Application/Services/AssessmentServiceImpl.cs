using AssessmentService.Application.DTOs;
using AssessmentService.Application.Interfaces;
using AssessmentService.Domain.Entities;
using AssessmentService.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using AssessmentService.Application.Mappings;
using Shared.Contracts;

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

        var assessment = request.ToEntity();    

        await _dbContext.GoalProbabilityAssessments.AddAsync(assessment);
        
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new AssessmentAlreadyExistsException(request.GoalId);
        }

        return assessment.ToResponse();
    }

    public async Task<AssessmentResponse> GetByIdAsync(Guid id)
    {
        var assessment = await _dbContext.GoalProbabilityAssessments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (assessment is null)
            throw new AssessmentNotFoundException(id);

        return assessment.ToResponse();
    }

    public async Task<AssessmentResponse?> GetByGoalIdAsync(Guid goalId)
    {
        var assessment = await _dbContext.GoalProbabilityAssessments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.GoalId == goalId);
        
        if (assessment is null)
            throw new AssessmentByGoalNotFoundException(goalId);

        return assessment.ToResponse();
    }

    public async Task<PaginationResponse<AssessmentResponse>> GetAllAsync(PaginationRequest pagination)
    {
        var query = _dbContext.GoalProbabilityAssessments.AsNoTracking();

        query = pagination.OrderBy?.ToLowerInvariant() switch
        {
            "probability" => query.OrderByDescending(x => x.Probability),
            "goalid" => query.OrderBy(x => x.GoalId),
            _ => query.OrderBy(x => x.GoalId)
        };

        var total = await query.CountAsync();

        var pageNumber = pagination.PageNumber < 1 ? 1 : pagination.PageNumber;
        var pageSize = pagination.PageSize < 1 ? 20 : pagination.PageSize;

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginationResponse<AssessmentResponse>
        {
            Items = items.Select(a => a.ToResponse()).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            Total = total
        };
    }

    public async Task<AssessmentResponse> UpdateAsync(Guid id, UpdateAssessmentRequest request)
    {
        var assessment = await _dbContext.GoalProbabilityAssessments
            .FirstOrDefaultAsync(a => a.Id == id);

        if (assessment is null)
            throw new AssessmentNotFoundException(id);

        request.UpdateEntity(assessment);

        await _dbContext.SaveChangesAsync();

        return assessment.ToResponse();
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
}
