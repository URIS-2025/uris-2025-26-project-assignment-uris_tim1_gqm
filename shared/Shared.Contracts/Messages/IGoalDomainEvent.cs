namespace Shared.Contracts.Messages;

/// <summary>
/// Published by GoalService when significant domain transitions occur (e.g. GoalCreated, GoalActivated).
/// Can be consumed by any interested service to react to goal lifecycle changes.
/// </summary>
public interface IGoalDomainEvent
{
    Guid CorrelationId { get; }
    Guid GoalId { get; }
    /// <summary>
    /// e.g. "GoalCreated", "GoalActivated", "GoalDeleted"
    /// </summary>
    string EventType { get; }
    string? Payload { get; }
    DateTime OccurredAt { get; }
}
