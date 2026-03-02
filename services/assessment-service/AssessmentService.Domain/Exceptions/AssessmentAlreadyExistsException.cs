namespace AssessmentService.Domain.Exceptions;

public class AssessmentAlreadyExistsException : Exception
{
    public AssessmentAlreadyExistsException(Guid goalId)
        : base($"Goal probability assessment for goal '{goalId}' already exists.")
    {
    }
}