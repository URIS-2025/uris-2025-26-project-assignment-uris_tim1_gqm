namespace PremiseService.Domain.Exceptions;

public class PremiseAlreadyDeactivatedException : Exception
{
    public PremiseAlreadyDeactivatedException(Guid id)
        : base($"Premise with ID '{id}' is already deactivated and cannot be updated. Update the latest active version instead.")
    {
    }
}
