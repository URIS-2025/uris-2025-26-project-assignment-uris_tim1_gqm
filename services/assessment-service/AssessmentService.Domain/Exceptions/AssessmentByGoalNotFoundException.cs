namespace AssessmentService.Domain.Exceptions;

public class AssessmentByGoalNotFoundException: Exception
{
    public AssessmentByGoalNotFoundException(Guid goalId)
        : base($"No assessment found for goal '{goalId}'.")
    {
    }
}