namespace PremiseService.Domain.Exceptions;

/// <summary>
/// Exception thrown when a premise with the specified identifier is not found in the system.
/// </summary>
public class PremiseNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="PremiseNotFoundException"/> with the given premise ID.
    /// </summary>
    /// <param name="id">The identifier of the premise that was not found.</param>
    public PremiseNotFoundException(Guid id)
        : base($"Premise with ID '{id}' was not found.")
    {
    }
}
