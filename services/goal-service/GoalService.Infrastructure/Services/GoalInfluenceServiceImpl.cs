using GoalService.Application.DTOs;
using GoalService.Application.Interfaces;
using GoalService.Application.Mappings;
using GoalService.Domain.Exceptions;
using GoalService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoalService.Infrastructure.Services;

public class GoalInfluenceServiceImpl : IGoalInfluenceService
{
    private readonly GoalDbContext _context;

    public GoalInfluenceServiceImpl(GoalDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<GoalInfluenceResponse>> GetByStrategyIdAsync(Guid strategyId)
    {
        var influences = await _context.GoalInfluences
            .Where(gi => gi.StrategyId == strategyId)
            .AsNoTracking()
            .ToListAsync();

        return influences.Select(gi => gi.ToResponse());
    }

    public async Task<GoalInfluenceResponse?> GetByGoalIdAsync(Guid goalId)
    {
        var influence = await _context.GoalInfluences
            .AsNoTracking()
            .FirstOrDefaultAsync(gi => gi.GoalId == goalId);

        return influence?.ToResponse();
    }

    public async Task<GoalInfluenceResponse> CreateAsync(GoalInfluenceRequest request)
    {
        // Verify goal exists
        var goalExists = await _context.Goals.AnyAsync(g => g.Id == request.GoalId);
        if (!goalExists)
            throw new GoalNotFoundException(request.GoalId);

        // Verify strategy exists
        var strategyExists = await _context.Strategies.AnyAsync(s => s.Id == request.StrategyId);
        if (!strategyExists)
            throw new StrategyNotFoundException(request.StrategyId);

        // Check if influence already exists for this goal (0..1 constraint)
        var existingInfluence = await _context.GoalInfluences
            .AnyAsync(gi => gi.GoalId == request.GoalId);
        if (existingInfluence)
            throw new InvalidGoalStateException(
                $"Goal '{request.GoalId}' already has an influence record. A goal can arise from at most one strategy.");

        // Cycle detection: Ensure adding this link does not create a cycle
        // Walk UP from Strategy's parent Goal to the root, checking if we encounter the target GoalId
        await DetectCycleAsync(request.GoalId, request.StrategyId);

        var influence = request.ToEntity();

        _context.GoalInfluences.Add(influence);
        await _context.SaveChangesAsync();

        return influence.ToResponse();
    }

    public async Task<bool> DeleteAsync(Guid goalId)
    {
        var influence = await _context.GoalInfluences.FindAsync(goalId);
        if (influence is null) return false;

        _context.GoalInfluences.Remove(influence);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Detect if creating a GoalInfluence from the strategy to the goal would form a cycle.
    /// We walk up the hierarchy: Strategy → parent Goal → its GoalInfluence → parent Strategy → parent Goal → ...
    /// If at any point we encounter the target goalId, there's a cycle.
    /// </summary>
    private async Task DetectCycleAsync(Guid targetGoalId, Guid strategyId)
    {
        // Find the goal that owns the strategy (the parent goal)
        var strategy = await _context.Strategies
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == strategyId);

        if (strategy is null) return;

        var currentGoalId = strategy.GoalId;
        var visited = new HashSet<Guid>();

        while (true)
        {
            if (currentGoalId == targetGoalId)
                throw new GoalHierarchyCycleException(targetGoalId, strategyId);

            if (!visited.Add(currentGoalId))
                break; // Already visited, no further up to go (safety check)

            // Check if current goal arose from a strategy (has a GoalInfluence)
            var parentInfluence = await _context.GoalInfluences
                .AsNoTracking()
                .FirstOrDefaultAsync(gi => gi.GoalId == currentGoalId);

            if (parentInfluence is null)
                break; // Reached the root, no cycle

            // Find the strategy that produced this goal, then find that strategy's parent goal
            var parentStrategy = await _context.Strategies
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == parentInfluence.StrategyId);

            if (parentStrategy is null)
                break; // Shouldn't happen, but defensive

            currentGoalId = parentStrategy.GoalId;
        }
    }
}
