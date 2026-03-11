namespace PremiseService.Domain.Exceptions;

public class PremiseNotFoundException : Exception
{
    public PremiseNotFoundException(Guid id)
        : base($"Premise with ID '{id}' was not found.")
    {
    }
}
