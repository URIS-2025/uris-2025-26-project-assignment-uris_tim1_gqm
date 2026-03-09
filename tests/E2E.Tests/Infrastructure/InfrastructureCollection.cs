namespace E2E.Tests.Infrastructure;

/// <summary>
/// Marks all test classes that need the shared PostgreSQL + RabbitMQ containers.
/// xunit creates <see cref="SharedInfrastructureFixture"/> once per process and shares it
/// across every class decorated with [Collection("Infrastructure")].
/// </summary>
[CollectionDefinition("Infrastructure")]
public sealed class InfrastructureCollection : ICollectionFixture<SharedInfrastructureFixture>
{
    // Marker class — no members needed.
}
