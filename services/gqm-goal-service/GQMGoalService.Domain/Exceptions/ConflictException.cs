namespace GQMGoalService.Domain.Exceptions;

/// <summary>
/// Thrown when an operation conflicts with the current state of a resource
/// (e.g., deleting an entity that has dependent children). Mapped to HTTP 409 Conflict.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message)
    {
    }
}
