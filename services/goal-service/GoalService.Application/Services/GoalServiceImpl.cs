using GoalService.Application.DTOs;
using Shared.Contracts;
using GoalService.Application.Interfaces;
using GoalService.Application.Interfaces.Clients;
using GoalService.Application.Mappings;
using GoalService.Domain.Enums;
using GoalService.Domain.Exceptions;
using GoalService.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using Shared.Contracts.Messages;

namespace GoalService.Application.Services;

public class GoalServiceImpl : IGoalService
{
    private readonly IGoalDbContext _context;
    private readonly IPremiseClient _premiseClient;
    private readonly IAssessmentClient _assessmentClient;
    private readonly IQgmGoalClient _qgmGoalClient;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IDepartmentClient _departmentClient;

    public GoalServiceImpl(
        IGoalDbContext context, 
        IPremiseClient premiseClient, 
        IAssessmentClient assessmentClient, 
        IQgmGoalClient qgmGoalClient,
        IPublishEndpoint publishEndpoint,
        IDepartmentClient departmentClient)
    {
        _context = context;
        _premiseClient = premiseClient;
        _assessmentClient = assessmentClient;
        _qgmGoalClient = qgmGoalClient;
        _publishEndpoint = publishEndpoint;
        _departmentClient = departmentClient;
    }

    public async Task<PaginationResponse<GoalResponse>> GetAllPaginatedAsync(PaginationRequest request)
    {
        var departmentIds = await _departmentClient.GetMyDepartmentIdsAsync();

        var query = _context.Goals
            .Include(g => g.Strategies)
                .ThenInclude(s => s.GoalInfluences)
            .Include(g => g.GoalInfluence)
            .Where(g => departmentIds.Contains(g.DepartmentId))
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

        await _publishEndpoint.Publish<IAuditLogCreated>(new
        {
            CorrelationId = Guid.NewGuid(),
            ActorId = Guid.Empty,
            ActorRole = "System",
            Service = "goal-service",
            Action = "GoalCreated",
            EntityType = "Goal",
            EntityId = goal.Id,
            Metadata = System.Text.Json.JsonSerializer.Serialize(new { goal.Focus }),
            OccurredAt = DateTime.UtcNow
        });

        await _publishEndpoint.Publish<IGoalDomainEvent>(new
        {
            CorrelationId = Guid.NewGuid(),
            GoalId = goal.Id,
            EventType = "GoalCreated",
            Payload = "{}",
            OccurredAt = DateTime.UtcNow
        });

        await _publishEndpoint.Publish<IWorkflowTransitionRequested>(new
        {
            CorrelationId = Guid.NewGuid(),
            GoalId = goal.Id,
            StepName = "StartWorkflow",
            CompensationEndpoint = "",
            CompensationPayload = "{}",
            RequestedAt = DateTime.UtcNow
        });

        await _publishEndpoint.Publish<IWorkflowTransitionRequested>(new
        {
            CorrelationId = Guid.NewGuid(),
            GoalId = goal.Id,
            StepName = "GoalCreated",
            CompensationEndpoint = $"api/Goal/{goal.Id}",
            CompensationPayload = "{}",
            RequestedAt = DateTime.UtcNow
        });

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

    public async Task<ActivationReadinessResponse> ReadinessAsync(Guid id)
    {
        var goal = await _context.Goals.FindAsync(id);
        if (goal is null)
            return new ActivationReadinessResponse(false, new[] { "Goal not found." });

        if (goal.Status != GoalStatus.Draft)
            return new ActivationReadinessResponse(false, new[] { $"Goal must be in Draft state to activate (current: {goal.Status})." });

        var blockers = await CollectActivationBlockersAsync(id);
        return new ActivationReadinessResponse(blockers.Count == 0, blockers);
    }

    public async Task<GoalResponse?> ActivateAsync(Guid id)
    {
        var goal = await _context.Goals.FindAsync(id);
        if (goal is null) return null;

        if (goal.Status != GoalStatus.Draft)
            throw new InvalidGoalStateException($"Goal '{id}' must be in Draft state to activate (current: {goal.Status}).");

        var blockers = await CollectActivationBlockersAsync(id);
        if (blockers.Any())
            throw new GoalActivationException(blockers);
            
        await _publishEndpoint.Publish<IWorkflowTransitionRequested>(new
        {
            CorrelationId = Guid.NewGuid(),
            GoalId = goal.Id,
            StepName = "Activated",
            CompensationEndpoint = $"api/Goal/{goal.Id}/revert-to-draft",
            CompensationPayload = "{}",
            RequestedAt = DateTime.UtcNow
        });

        goal.Status = GoalStatus.Active;
        await _context.SaveChangesAsync();

        return goal.ToResponse();
    }

    public async Task<GoalResponse?> RevertToDraftAsync(Guid id)
    {
        var goal = await _context.Goals.FindAsync(id);
        if (goal is null) return null;

        goal.Status = GoalStatus.Draft;
        await _context.SaveChangesAsync();
        return goal.ToResponse();
    }

    private async Task<List<string>> CollectActivationBlockersAsync(Guid goalId)
    {
        var blockers = new List<string>();

        var hasActiveStrategy = await _context.Strategies
            .AnyAsync(s => s.GoalId == goalId && s.IsActive);
        if (!hasActiveStrategy)
            blockers.Add("No active strategy is defined for this goal.");

        var assessments = await _assessmentClient.GetAssessmentsForGoalAsync(goalId);
        var assessment = assessments.FirstOrDefault();
        if (assessment is null)
            blockers.Add("No assessment has been created for this goal.");
        else if (!string.Equals(assessment.State, "Completed", StringComparison.OrdinalIgnoreCase))
            blockers.Add($"Assessment is not in Completed state (current: {assessment.State}).");

        var qgmGoals = await _qgmGoalClient.GetQgmGoalsForGoalAsync(goalId);
        if (!qgmGoals.Any())
            blockers.Add("No GQM structure (Goal-Question-Metric) is defined for this goal.");

        return blockers;
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
