using Shared.Contracts;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using GQMGoalService.Application.DTOs;
using GQMGoalService.Application.DTOs.GqmGoal;
using GQMGoalService.Application.Interfaces;
using GQMGoalService.Domain.Entities;
using GQMGoalService.Domain.Exceptions;
using FluentValidation;

namespace GQMGoalService.Application.Services;

public class GqmGoalService : IGqmGoalService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IValidator<GqmGoalRequest> _validator;

    public GqmGoalService(IApplicationDbContext dbContext, IMapper mapper, IValidator<GqmGoalRequest> validator)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<PaginationResponse<GqmGoalResponse>> GetAllAsync(PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.GqmGoals
            .Include(g => g.Questions)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.OrderBy))
        {
            query = request.OrderBy.ToLower() switch
            {
                "description" => query.OrderBy(g => g.Description),
                "createdat" => query.OrderBy(g => g.CreatedAt),
                _ => query.OrderBy(g => g.Id)
            };
        }
        else
        {
            query = query.OrderBy(g => g.Id);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var goals = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
            
        var dtos = _mapper.Map<IEnumerable<GqmGoalResponse>>(goals);
        return new PaginationResponse<GqmGoalResponse>
        {
            Items = dtos,
            Total = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<GqmGoalResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var goal = await _dbContext.GqmGoals
            .Include(g => g.Questions)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
            
        if (goal == null)
            throw new NotFoundException(nameof(GqmGoal), id);

        return _mapper.Map<GqmGoalResponse>(goal);
    }

    public async Task<IEnumerable<GqmGoalResponse>> GetByGoalIdAsync(Guid goalId, CancellationToken cancellationToken = default)
    {
        var goals = await _dbContext.GqmGoals
            .Include(g => g.Questions)
            .Where(g => g.GoalId == goalId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (goals.Count == 0)
            throw new NotFoundException("GqmGoal", $"GoalId: {goalId}");
            
        return _mapper.Map<IEnumerable<GqmGoalResponse>>(goals);
    }

    public async Task<GqmGoalResponse> CreateAsync(GqmGoalRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var goal = _mapper.Map<GqmGoal>(request);
        goal.CreatedAt = DateTime.UtcNow;

        _dbContext.GqmGoals.Add(goal);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<GqmGoalResponse>(goal);
    }

    public async Task<GqmGoalResponse> UpdateAsync(Guid id, GqmGoalRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var goal = await _dbContext.GqmGoals.FindAsync(new object[] { id }, cancellationToken);
        if (goal == null)
            throw new NotFoundException(nameof(GqmGoal), id);

        _mapper.Map(request, goal);
        
        _dbContext.GqmGoals.Update(goal);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<GqmGoalResponse>(goal);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var goal = await _dbContext.GqmGoals.FindAsync(new object[] { id }, cancellationToken);
        if (goal == null)
            throw new NotFoundException(nameof(GqmGoal), id);

        bool hasQuestions = await _dbContext.Questions.AnyAsync(q => q.GqmGoalId == id, cancellationToken);
        if (hasQuestions)
            throw new ConflictException("Cannot delete GqmGoal because it has associated questions.");

        _dbContext.GqmGoals.Remove(goal);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}
