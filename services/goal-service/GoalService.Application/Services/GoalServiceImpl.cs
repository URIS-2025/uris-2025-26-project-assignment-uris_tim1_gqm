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

    // ========== Analytics Methods ==========

    public async Task<IEnumerable<GoalResponse>> GetRootGoalsByDepartmentAsync(Guid departmentId)
    {
        // Root goals are goals that have no GoalInfluence (not derived from any strategy)
        var rootGoals = await _context.Goals
            .Include(g => g.Strategies)
                .ThenInclude(s => s.GoalInfluences)
            .Include(g => g.GoalInfluence)
            .Where(g => g.DepartmentId == departmentId && g.GoalInfluence == null)
            .AsNoTracking()
            .ToListAsync();

        return rootGoals.Select(g => g.ToResponse());
    }

    public async Task<GoalTreeNodeResponse?> GetGoalTreeAsync(Guid rootGoalId)
    {
        var rootGoal = await _context.Goals
            .Include(g => g.Strategies)
                .ThenInclude(s => s.GoalInfluences)
            .Include(g => g.GoalInfluence)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == rootGoalId);

        if (rootGoal is null)
            return null;

        return await BuildGoalTreeNodeAsync(rootGoal);
    }

    private async Task<GoalTreeNodeResponse> BuildGoalTreeNodeAsync(Domain.Entities.Goal goal)
    {
        var strategyNodes = new List<StrategyTreeNodeResponse>();

        foreach (var strategy in goal.Strategies)
        {
            var childGoals = new List<ChildGoalInfluenceResponse>();

            // Get all goals influenced by this strategy
            var influences = await _context.GoalInfluences
                .Include(gi => gi.Goal)
                    .ThenInclude(g => g.Strategies)
                        .ThenInclude(s => s.GoalInfluences)
                .Where(gi => gi.StrategyId == strategy.Id)
                .AsNoTracking()
                .ToListAsync();

            foreach (var influence in influences)
            {
                // Recursively build the child goal tree
                var childGoalNode = await BuildGoalTreeNodeAsync(influence.Goal);
                childGoals.Add(new ChildGoalInfluenceResponse
                {
                    Goal = childGoalNode,
                    InfluenceType = influence.InfluenceType.ToString(),
                    Strength = influence.Strength,
                    Confidence = influence.Confidence,
                    Notes = influence.Notes
                });
            }

            strategyNodes.Add(new StrategyTreeNodeResponse
            {
                Id = strategy.Id,
                Name = strategy.Name,
                Description = strategy.Description,
                RefinementType = strategy.RefinementType.ToString(),
                Effectiveness = strategy.Effectiveness.ToString(),
                IsActive = strategy.IsActive,
                ChildGoals = childGoals
            });
        }

        return new GoalTreeNodeResponse
        {
            Id = goal.Id,
            Focus = goal.Focus,
            Object = goal.Object,
            Status = goal.Status.ToString(),
            BaselineProbability = goal.BaselineProbability,
            DepartmentId = goal.DepartmentId,
            ActiveFrom = goal.ActiveFrom,
            ActiveTo = goal.ActiveTo,
            Magnitude = goal.Magnitude,
            Constraints = goal.Constraints,
            Strategies = strategyNodes
        };
    }

    public async Task<GoalAnalyticsResponse> GetAnalyticsAsync(Guid? departmentId, Guid? rootGoalId)
    {
        List<Domain.Entities.Goal> goals;
        List<Domain.Entities.Strategy> strategies;
        var depthMap = new Dictionary<Guid, int>();

        if (rootGoalId.HasValue)
        {
            // Get all goals in the tree rooted at rootGoalId
            goals = await GetAllGoalsInTreeAsync(rootGoalId.Value, depthMap, 0);
            strategies = goals.SelectMany(g => g.Strategies).ToList();
        }
        else if (departmentId.HasValue)
        {
            // Get all goals for the department
            goals = await _context.Goals
                .Include(g => g.Strategies)
                .Include(g => g.GoalInfluence)
                .Where(g => g.DepartmentId == departmentId.Value)
                .AsNoTracking()
                .ToListAsync();
            strategies = goals.SelectMany(g => g.Strategies).ToList();
            
            // Calculate depth for department-scoped goals
            var rootGoals = goals.Where(g => g.GoalInfluence == null).ToList();
            foreach (var root in rootGoals)
            {
                await CalculateDepthsAsync(root.Id, depthMap, 0);
            }
        }
        else
        {
            // Get all goals for user's departments
            var departmentIds = await _departmentClient.GetMyDepartmentIdsAsync();
            goals = await _context.Goals
                .Include(g => g.Strategies)
                .Include(g => g.GoalInfluence)
                .Where(g => departmentIds.Contains(g.DepartmentId))
                .AsNoTracking()
                .ToListAsync();
            strategies = goals.SelectMany(g => g.Strategies).ToList();
            
            // Calculate depth for all goals
            var rootGoals = goals.Where(g => g.GoalInfluence == null).ToList();
            foreach (var root in rootGoals)
            {
                await CalculateDepthsAsync(root.Id, depthMap, 0);
            }
        }

        // KPIs
        var totalGoals = goals.Count;
        var activeGoals = goals.Count(g => g.Status == GoalStatus.Active);
        var completedGoals = goals.Count(g => g.Status == GoalStatus.Completed);
        var draftGoals = goals.Count(g => g.Status == GoalStatus.Draft);

        // Status distribution
        var statusDistribution = goals
            .GroupBy(g => g.Status.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        // Probability distribution (5 buckets)
        var probabilityDistribution = new Dictionary<string, int>
        {
            { "0-20%", goals.Count(g => g.BaselineProbability >= 0 && g.BaselineProbability < 0.2m) },
            { "20-40%", goals.Count(g => g.BaselineProbability >= 0.2m && g.BaselineProbability < 0.4m) },
            { "40-60%", goals.Count(g => g.BaselineProbability >= 0.4m && g.BaselineProbability < 0.6m) },
            { "60-80%", goals.Count(g => g.BaselineProbability >= 0.6m && g.BaselineProbability < 0.8m) },
            { "80-100%", goals.Count(g => g.BaselineProbability >= 0.8m && g.BaselineProbability <= 1.0m) }
        };

        // Depth distribution
        var depthDistribution = depthMap
            .GroupBy(kv => kv.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        // Refinement distribution
        var refinementDistribution = strategies
            .GroupBy(s => s.RefinementType.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        // Insights
        var highestProbGoal = goals
            .OrderByDescending(g => g.BaselineProbability)
            .FirstOrDefault();

        var lowestProbActiveGoal = goals
            .Where(g => g.Status == GoalStatus.Active)
            .OrderBy(g => g.BaselineProbability)
            .FirstOrDefault();

        var mostProductiveStrategy = strategies
            .Select(s => new { Strategy = s, ChildCount = s.GoalInfluences.Count })
            .OrderByDescending(x => x.ChildCount)
            .FirstOrDefault();

        // Most active department (only for org-wide analytics)
        DepartmentInsightResponse? mostActiveDept = null;
        if (!departmentId.HasValue && !rootGoalId.HasValue)
        {
            var deptGroup = goals
                .Where(g => g.Status == GoalStatus.Active)
                .GroupBy(g => g.DepartmentId)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            if (deptGroup != null)
            {
                var deptInfo = await _departmentClient.GetDepartmentAsync(deptGroup.Key);
                if (deptInfo != null)
                {
                    mostActiveDept = new DepartmentInsightResponse
                    {
                        Id = deptGroup.Key,
                        Name = deptInfo.Name,
                        ActiveGoalsCount = deptGroup.Count()
                    };
                }
            }
        }

        return new GoalAnalyticsResponse
        {
            TotalGoals = totalGoals,
            ActiveGoals = activeGoals,
            CompletedGoals = completedGoals,
            DraftGoals = draftGoals,
            StatusDistribution = statusDistribution,
            ProbabilityDistribution = probabilityDistribution,
            DepthDistribution = depthDistribution,
            RefinementDistribution = refinementDistribution,
            HighestProbabilityGoal = highestProbGoal != null ? new GoalInsightResponse
            {
                Id = highestProbGoal.Id,
                Focus = highestProbGoal.Focus,
                Object = highestProbGoal.Object,
                Status = highestProbGoal.Status.ToString(),
                BaselineProbability = highestProbGoal.BaselineProbability,
                DepartmentId = highestProbGoal.DepartmentId
            } : null,
            LowestProbabilityActiveGoal = lowestProbActiveGoal != null ? new GoalInsightResponse
            {
                Id = lowestProbActiveGoal.Id,
                Focus = lowestProbActiveGoal.Focus,
                Object = lowestProbActiveGoal.Object,
                Status = lowestProbActiveGoal.Status.ToString(),
                BaselineProbability = lowestProbActiveGoal.BaselineProbability,
                DepartmentId = lowestProbActiveGoal.DepartmentId
            } : null,
            MostProductiveStrategy = mostProductiveStrategy?.ChildCount > 0 ? new StrategyInsightResponse
            {
                Id = mostProductiveStrategy.Strategy.Id,
                Name = mostProductiveStrategy.Strategy.Name,
                GoalId = mostProductiveStrategy.Strategy.GoalId,
                GoalFocus = goals.FirstOrDefault(g => g.Id == mostProductiveStrategy.Strategy.GoalId)?.Focus ?? "",
                DerivedGoalsCount = mostProductiveStrategy.ChildCount
            } : null,
            MostActiveDepartment = mostActiveDept
        };
    }

    private async Task<List<Domain.Entities.Goal>> GetAllGoalsInTreeAsync(Guid goalId, Dictionary<Guid, int> depthMap, int depth)
    {
        var goal = await _context.Goals
            .Include(g => g.Strategies)
                .ThenInclude(s => s.GoalInfluences)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == goalId);

        if (goal is null)
            return [];

        var result = new List<Domain.Entities.Goal> { goal };
        depthMap[goal.Id] = depth;

        foreach (var strategy in goal.Strategies)
        {
            foreach (var influence in strategy.GoalInfluences)
            {
                var childGoals = await GetAllGoalsInTreeAsync(influence.GoalId, depthMap, depth + 1);
                result.AddRange(childGoals);
            }
        }

        return result;
    }

    private async Task CalculateDepthsAsync(Guid goalId, Dictionary<Guid, int> depthMap, int depth)
    {
        if (depthMap.ContainsKey(goalId))
            return;

        depthMap[goalId] = depth;

        var strategies = await _context.Strategies
            .Include(s => s.GoalInfluences)
            .Where(s => s.GoalId == goalId)
            .AsNoTracking()
            .ToListAsync();

        foreach (var strategy in strategies)
        {
            foreach (var influence in strategy.GoalInfluences)
            {
                await CalculateDepthsAsync(influence.GoalId, depthMap, depth + 1);
            }
        }
    }
}
