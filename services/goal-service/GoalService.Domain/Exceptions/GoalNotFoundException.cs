namespace GoalService.Domain.Exceptions;

public class GoalNotFoundException : Exception
{
    public GoalNotFoundException(Guid goalId)
        : base($"Goal with ID '{goalId}' was not found.") { }
}
