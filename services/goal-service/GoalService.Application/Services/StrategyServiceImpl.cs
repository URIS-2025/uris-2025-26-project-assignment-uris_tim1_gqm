using GoalService.Application.DTOs;
using GoalService.Application.Interfaces;
using GoalService.Application.Mappings;
using GoalService.Domain.Exceptions;
using GoalService.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoalService.Application.Services;

public class StrategyServiceImpl : IStrategyService
{
    private readonly IGoalDbContext _context;

    public StrategyServiceImpl(IGoalDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<StrategyResponse>> GetByGoalIdAsync(Guid goalId)
    {
        var strategies = await _context.Strategies
            .Include(s => s.GoalInfluences)
            .Where(s => s.GoalId == goalId)
            .AsNoTracking()
            .ToListAsync();

        return strategies.Select(s => s.ToResponse());
    }

    public async Task<StrategyResponse?> GetByIdAsync(Guid id)
    {
        var strategy = await _context.Strategies
            .Include(s => s.GoalInfluences)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        return strategy?.ToResponse();
    }

    public async Task<StrategyResponse> CreateAsync(StrategyRequest request)
    {
        // Verify parent goal exists
        var goalExists = await _context.Goals.AnyAsync(g => g.Id == request.GoalId);
        if (!goalExists)
            throw new GoalNotFoundException(request.GoalId);

        var strategy = request.ToEntity();

        _context.Strategies.Add(strategy);
        await _context.SaveChangesAsync();

        return strategy.ToResponse();
    }

    public async Task<StrategyResponse?> UpdateAsync(Guid id, StrategyRequest request)
    {
        var strategy = await _context.Strategies.FindAsync(id);
        if (strategy is null) return null;

        // Verify new parent goal exists
        var goalExists = await _context.Goals.AnyAsync(g => g.Id == request.GoalId);
        if (!goalExists)
            throw new GoalNotFoundException(request.GoalId);

        request.UpdateEntity(strategy);
        await _context.SaveChangesAsync();

        // Reload with includes
        var updated = await _context.Strategies
            .Include(s => s.GoalInfluences)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        return updated?.ToResponse();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var strategy = await _context.Strategies.FindAsync(id);
        if (strategy is null) return false;

        _context.Strategies.Remove(strategy);
        await _context.SaveChangesAsync();
        return true;
    }
}
