using AutoMapper;
using Microsoft.EntityFrameworkCore;
using GQMGoalService.Application.DTOs;
using GQMGoalService.Application.DTOs.GqmGoal;
using GQMGoalService.Application.Interfaces;
using GQMGoalService.Domain.Entities;
using GQMGoalService.Domain.Exceptions;
using GQMGoalService.Infrastructure.Persistence;
using FluentValidation;

namespace GQMGoalService.Application.Services;

public class GqmGoalService : IGqmGoalService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IValidator<GqmGoalRequest> _validator;

    public GqmGoalService(ApplicationDbContext dbContext, IMapper mapper, IValidator<GqmGoalRequest> validator)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<PagedResult<GqmGoalResponse>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        var totalCount = await _dbContext.GqmGoals.CountAsync();
        var goals = await _dbContext.GqmGoals
            .Include(g => g.Questions)
            .AsNoTracking()
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        var dtos = _mapper.Map<IEnumerable<GqmGoalResponse>>(goals);
        return new PagedResult<GqmGoalResponse>(dtos, totalCount, pageNumber, pageSize);
    }

    public async Task<GqmGoalResponse> GetByIdAsync(Guid id)
    {
        var goal = await _dbContext.GqmGoals
            .Include(g => g.Questions)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id);
            
        if (goal == null)
            throw new NotFoundException(nameof(GqmGoal), id);

        return _mapper.Map<GqmGoalResponse>(goal);
    }

    public async Task<IEnumerable<GqmGoalResponse>> GetByGoalIdAsync(Guid goalId)
    {
        var goals = await _dbContext.GqmGoals
            .Include(g => g.Questions)
            .Where(g => g.GoalId == goalId)
            .AsNoTracking()
            .ToListAsync();
            
        return _mapper.Map<IEnumerable<GqmGoalResponse>>(goals);
    }

    public async Task<GqmGoalResponse> CreateAsync(GqmGoalRequest request)
    {
        await _validator.ValidateAndThrowAsync(request);

        var goal = _mapper.Map<GqmGoal>(request);
        goal.CreatedAt = DateTime.UtcNow;

        _dbContext.GqmGoals.Add(goal);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<GqmGoalResponse>(goal);
    }

    public async Task<GqmGoalResponse> UpdateAsync(Guid id, GqmGoalRequest request)
    {
        await _validator.ValidateAndThrowAsync(request);

        var goal = await _dbContext.GqmGoals.FindAsync(id);
        if (goal == null)
            throw new NotFoundException(nameof(GqmGoal), id);

        _mapper.Map(request, goal);
        
        _dbContext.GqmGoals.Update(goal);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<GqmGoalResponse>(goal);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var goal = await _dbContext.GqmGoals.FindAsync(id);
        if (goal == null)
            throw new NotFoundException(nameof(GqmGoal), id);

        bool hasQuestions = await _dbContext.Questions.AnyAsync(q => q.GqmGoalId == id);
        if (hasQuestions)
            throw new InvalidOperationException("Cannot delete GqmGoal because it has associated questions.");

        _dbContext.GqmGoals.Remove(goal);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}
