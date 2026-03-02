namespace AssessmentService.Domain.Exceptions;
public class InvalidProbabilityException : Exception
{
    public InvalidProbabilityException(decimal probability)
        : base($"Probability value '{probability}' is invalid. It must be between 0 and 1.")
    {
    }
}