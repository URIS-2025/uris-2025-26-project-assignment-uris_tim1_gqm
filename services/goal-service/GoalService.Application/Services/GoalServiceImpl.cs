using GoalService.Application.DTOs;
using GoalService.Application.Interfaces;
using GoalService.Application.Mappings;
using GoalService.Domain.Exceptions;
using GoalService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoalService.Application.Services;

public class GoalServiceImpl : IGoalService
{
    private readonly GoalDbContext _context;

    public GoalServiceImpl(GoalDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<GoalResponse>> GetAllAsync()
    {
        var goals = await _context.Goals
            .Include(g => g.Strategies)
                .ThenInclude(s => s.GoalInfluences)
            .Include(g => g.GoalInfluence)
            .AsNoTracking()
            .ToListAsync();

        return goals.Select(g => g.ToResponse());
    }

    public async Task<GoalResponse?> GetByIdAsync(Guid id)
    {
        var goal = await _context.Goals
            .Include(g => g.Strategies)
                .ThenInclude(s => s.GoalInfluences)
            .Include(g => g.GoalInfluence)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id);

        return goal?.ToResponse();
    }

    public async Task<GoalResponse> CreateAsync(GoalRequest request)
    {
        var goal = request.ToEntity();

        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        return goal.ToResponse();
    }

    public async Task<GoalResponse?> UpdateAsync(Guid id, GoalRequest request)
    {
        var goal = await _context.Goals.FindAsync(id);
        if (goal is null) return null;

        request.UpdateEntity(goal);
        await _context.SaveChangesAsync();

        // Reload with includes for full response
        var updated = await _context.Goals
            .Include(g => g.Strategies)
                .ThenInclude(s => s.GoalInfluences)
            .Include(g => g.GoalInfluence)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id);

        return updated?.ToResponse();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var goal = await _context.Goals.FindAsync(id);
        if (goal is null) return false;

        _context.Goals.Remove(goal);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<GoalResponse>> GetByDepartmentIdAsync(Guid departmentId)
    {
        var goals = await _context.Goals
            .Include(g => g.Strategies)
                .ThenInclude(s => s.GoalInfluences)
            .Include(g => g.GoalInfluence)
            .Where(g => g.DepartmentId == departmentId)
            .AsNoTracking()
            .ToListAsync();

        return goals.Select(g => g.ToResponse());
    }
}
