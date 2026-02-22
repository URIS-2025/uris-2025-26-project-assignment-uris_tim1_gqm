using AutoMapper;
using Microsoft.EntityFrameworkCore;
using GQMGoalService.Application.DTOs.GqmGoal;
using GQMGoalService.Application.Interfaces;
using GQMGoalService.Domain.Entities;
using GQMGoalService.Domain.Exceptions;
using GQMGoalService.Infrastructure.Persistence;

namespace GQMGoalService.Application.Services;

public class GqmGoalService : IGqmGoalService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public GqmGoalService(ApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<IEnumerable<GqmGoalResponse>> GetAllAsync()
    {
        var goals = await _dbContext.GqmGoals
            .Include(g => g.Questions)
            .ToListAsync();
        return _mapper.Map<IEnumerable<GqmGoalResponse>>(goals);
    }

    public async Task<GqmGoalResponse> GetByIdAsync(Guid id)
    {
        var goal = await _dbContext.GqmGoals
            .Include(g => g.Questions)
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
            .ToListAsync();
            
        return _mapper.Map<IEnumerable<GqmGoalResponse>>(goals);
    }

    public async Task<GqmGoalResponse> CreateAsync(GqmGoalRequest request)
    {
        var goal = _mapper.Map<GqmGoal>(request);
        goal.CreatedAt = DateTime.UtcNow;

        _dbContext.GqmGoals.Add(goal);
        await _dbContext.SaveChangesAsync();

        return _mapper.Map<GqmGoalResponse>(goal);
    }

    public async Task<GqmGoalResponse> UpdateAsync(Guid id, GqmGoalRequest request)
    {
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

        _dbContext.GqmGoals.Remove(goal);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}
