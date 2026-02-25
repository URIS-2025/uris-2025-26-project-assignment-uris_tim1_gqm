namespace AssessmentService.Domain.Exceptions;

public class AssessmentNotFoundException : Exception
{
    public AssessmentNotFoundException(Guid id)
        : base($"Goal probability assessment with ID '{id}' was not found.")
    {
    }
}
