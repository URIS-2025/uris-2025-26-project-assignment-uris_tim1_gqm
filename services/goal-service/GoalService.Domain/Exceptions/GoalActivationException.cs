namespace GoalService.Domain.Exceptions;

public class GoalActivationException : Exception
{
    public IReadOnlyList<string> Blockers { get; }

    public GoalActivationException(IReadOnlyList<string> blockers)
        : base("Goal cannot be activated because prerequisites are not met.")
    {
        Blockers = blockers;
    }
}
