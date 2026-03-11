namespace AuditService.Domain.Exceptions;

public class AuditLogNotFoundException : Exception
{
    public AuditLogNotFoundException(Guid id)
        : base($"Audit log with id '{id}' was not found.") { }
}
