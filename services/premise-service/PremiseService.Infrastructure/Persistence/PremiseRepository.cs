using Microsoft.EntityFrameworkCore;
using PremiseService.Application.Interfaces;
using PremiseService.Domain.Entities;

namespace PremiseService.Infrastructure.Persistence;

public class PremiseRepository : IPremiseRepository
{
    private readonly PremiseDbContext _context;

    public PremiseRepository(PremiseDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Premise>> GetAllActiveAsync()
    {
        return await _context.Premises
            .Where(p => p.IsActive)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Premise?> GetByIdAsync(Guid id)
    {
        return await _context.Premises
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Premise>> GetByGoalIdAsync(Guid goalId)
    {
        return await _context.Premises
            .Where(p => p.GoalId == goalId && p.IsActive)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Premise>> GetByStrategyIdAsync(Guid strategyId)
    {
        return await _context.Premises
            .Where(p => p.StrategyId == strategyId && p.IsActive)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Premise>> GetVersionHistoryAsync(Guid premiseId)
    {
        var history = new List<Premise>();

        var current = await _context.Premises
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == premiseId);

        if (current == null)
            return history;

        history.Add(current);

        // Walk backwards through version chain
        var previousId = current.NewVersionOfId;
        while (previousId.HasValue)
        {
            var previous = await _context.Premises
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == previousId.Value);

            if (previous == null)
                break;

            history.Add(previous);
            previousId = previous.NewVersionOfId;
        }

        // Reverse so oldest is first
        history.Reverse();
        return history;
    }

    public async Task<Premise> CreateAsync(Premise premise)
    {
        await _context.Premises.AddAsync(premise);
        return premise;
    }

    public Task UpdateAsync(Premise premise)
    {
        _context.Premises.Update(premise);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
