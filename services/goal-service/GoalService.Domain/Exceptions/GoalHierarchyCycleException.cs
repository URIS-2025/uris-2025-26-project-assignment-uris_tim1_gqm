namespace GoalService.Domain.Exceptions;

/// <summary>
/// Thrown when an operation would create a cycle in the goal hierarchy.
/// The goal→strategy→goal structure must form a tree (DAG) — no cycles allowed.
/// </summary>
public class GoalHierarchyCycleException : Exception
{
    public GoalHierarchyCycleException(Guid goalId, Guid strategyId)
        : base($"Creating a link from Strategy '{strategyId}' to Goal '{goalId}' would create a cycle in the goal hierarchy.") { }
}
