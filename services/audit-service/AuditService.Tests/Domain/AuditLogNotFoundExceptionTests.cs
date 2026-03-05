using AuditService.Domain.Exceptions;

namespace AuditService.Tests.Domain;

public class AuditLogNotFoundExceptionTests
{
    [Fact]
    public void Constructor_ShouldSetMessage_WithProvidedId()
    {
        var id = Guid.NewGuid();

        var exception = new AuditLogNotFoundException(id);

        Assert.Equal($"Audit log with id '{id}' was not found.", exception.Message);
    }

    [Fact]
    public void Constructor_ShouldInheritFromException()
    {
        var exception = new AuditLogNotFoundException(Guid.NewGuid());

        Assert.IsAssignableFrom<Exception>(exception);
    }

    [Fact]
    public void Constructor_ShouldProduceDifferentMessages_ForDifferentIds()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        var ex1 = new AuditLogNotFoundException(id1);
        var ex2 = new AuditLogNotFoundException(id2);

        Assert.NotEqual(ex1.Message, ex2.Message);
    }
}
