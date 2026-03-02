namespace GQMGoalService.Domain.Exceptions;

/// <summary>
/// Thrown when domain-level validation fails. Carries per-property error details.
/// Mapped to HTTP 400 Bad Request.
/// </summary>
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation failures have occurred.")
    {
        Errors = errors;
    }
}
