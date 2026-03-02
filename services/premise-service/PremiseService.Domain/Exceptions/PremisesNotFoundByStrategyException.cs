namespace PremiseService.Domain.Exceptions;

public class PremisesNotFoundByStrategyException : Exception
{
    public PremisesNotFoundByStrategyException(Guid strategyId)
        : base($"No active premises found for Strategy with ID '{strategyId}'.")
    {
    }
}
