namespace GQMGoalService.Domain.Exceptions;

/// <summary>
/// Thrown when a requested entity cannot be found. Mapped to HTTP 404 Not Found.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"Entity \"{entityName}\" ({key}) was not found.")
    {
    }
}
