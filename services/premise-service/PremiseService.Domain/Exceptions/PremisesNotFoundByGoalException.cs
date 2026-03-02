namespace PremiseService.Domain.Exceptions;

public class PremisesNotFoundByGoalException : Exception
{
    public PremisesNotFoundByGoalException(Guid goalId)
        : base($"No active premises found for Goal with ID '{goalId}'.")
    {
    }
}
