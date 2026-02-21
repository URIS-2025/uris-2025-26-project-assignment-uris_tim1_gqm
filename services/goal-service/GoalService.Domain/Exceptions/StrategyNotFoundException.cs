namespace GoalService.Domain.Exceptions;

public class StrategyNotFoundException : Exception
{
    public StrategyNotFoundException(Guid strategyId)
        : base($"Strategy with ID '{strategyId}' was not found.") { }
}
