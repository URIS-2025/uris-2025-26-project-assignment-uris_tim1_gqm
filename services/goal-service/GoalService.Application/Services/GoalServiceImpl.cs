using GoalService.Application.DTOs;
using Shared.Contracts;
using GoalService.Application.Interfaces;
using GoalService.Application.Interfaces.Clients;
using GoalService.Application.Mappings;
using GoalService.Domain.Exceptions;
using GoalService.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoalService.Application.Services;

public class GoalServiceImpl : IGoalService
{
    private readonly IGoalDbContext _context;
    private readonly IPremiseClient _premiseClient;
    private readonly IAssessmentClient _assessmentClient;
    private readonly IQgmGoalClient _qgmGoalClient;

    public GoalServiceImpl(
        IGoalDbContext context, 
        IPremiseClient premiseClient, 
        IAssessmentClient assessmentClient, 
        IQgmGoalClient qgmGoalClient)
    {
        _context = context;
        _premiseClient = premiseClient;
        _assessmentClient = assessmentClient;
        _qgmGoalClient = qgmGoalClient;
    }

    public async Task<PaginationResponse<GoalResponse>> GetAllPaginatedAsync(PaginationRequest request)
    {
        var query = _context.Goals
            .Include(g => g.Strategies)
                .ThenInclude(s => s.GoalInfluences)
            .Include(g => g.GoalInfluence)
            .AsNoTracking();

        // Optional: Simple OrderBy implementation
        if (!string.IsNullOrWhiteSpace(request.OrderBy))
        {
            query = request.OrderBy.ToLower() switch
            {
                "focus" => query.OrderBy(g => g.Focus),
                "activefrom" => query.OrderBy(g => g.ActiveFrom),
                "status" => query.OrderBy(g => g.Status),
                _ => query.OrderBy(g => g.Id) // Default fallback
            };
        }
        else
        {
            query = query.OrderBy(g => g.Id); // Default sort
        }

        var total = await query.CountAsync();

        var goals = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PaginationResponse<GoalResponse>
        {
            Items = goals.Select(g => g.ToResponse()),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            Total = total
        };
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

    public async Task<GoalDetailsResponse?> GetGoalDetailsAsync(Guid id)
    {
        var goalInfo = await GetByIdAsync(id);
        if (goalInfo is null)
            return null;

        // Fetch external data concurrently
        var premisesTask = _premiseClient.GetPremisesForGoalAsync(id);
        var assessmentsTask = _assessmentClient.GetAssessmentsForGoalAsync(id);
        var qgmGoalsTask = _qgmGoalClient.GetQgmGoalsForGoalAsync(id);

        await Task.WhenAll(premisesTask, assessmentsTask, qgmGoalsTask);

        return new GoalDetailsResponse
        {
            Id = goalInfo.Id,
            Focus = goalInfo.Focus,
            Object = goalInfo.Object,
            ActiveFrom = goalInfo.ActiveFrom,
            ActiveTo = goalInfo.ActiveTo,
            Magnitude = goalInfo.Magnitude,
            Constraints = goalInfo.Constraints,
            Status = goalInfo.Status,
            BaselineProbability = goalInfo.BaselineProbability,
            DepartmentId = goalInfo.DepartmentId,
            Strategies = goalInfo.Strategies,
            GoalInfluence = goalInfo.GoalInfluence,
            Premises = premisesTask.Result,
            Assessments = assessmentsTask.Result,
            QgmGoals = qgmGoalsTask.Result
        };
    }
}
