namespace GoalService.Domain.Exceptions;

public class InvalidGoalStateException : Exception
{
    public InvalidGoalStateException(string message)
        : base(message) { }
}
