using DotNet.Testcontainers.Builders;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace E2E.Tests.Infrastructure;

/// <summary>
/// Starts a single PostgreSQL and a single RabbitMQ container that are shared across
/// all test classes within the [Collection("Infrastructure")] collection.
/// Containers are started once and torn down once — significantly faster than per-class setup.
/// </summary>
public sealed class SharedInfrastructureFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("postgres")         // we create named DBs on top of this
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        .Build();

    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management-alpine")
        .WithUsername("guest")
        .WithPassword("guest")
        .WithPortBinding(5672, true)
        .WithPortBinding(15672, true)
        .Build();

    // ── Exposed properties ───────────────────────────────────────────────────
    
    public string RabbitMqManagementUri => 
        $"http://localhost:{_rabbitMq.GetMappedPublicPort(15672)}";

    /// <summary>
    /// Returns a Npgsql-compatible connection string for <paramref name="dbName"/>.
    /// </summary>
    public string GetPostgresConnectionString(string dbName)
    {
        // More robust replacement: look for "Database=postgres" and replace with "Database=dbName"
        // disregarding trailing semicolon presence.
        var connectionString = _postgres.GetConnectionString();
        return connectionString.Replace("Database=postgres", $"Database={dbName}");
    }

    /// <summary>
    /// Explicitly creates a new database in the shared container.
    /// Npgsql's MigrateAsync does not create databases automatically.
    /// </summary>
    public async Task CreateDatabaseAsync(string dbName)
    {
        // ExecScriptAsync runs against the default 'postgres' database in the container.
        await _postgres.ExecScriptAsync($@"
            DO $$
            BEGIN
                IF NOT EXISTS (SELECT FROM pg_database WHERE datname = '{dbName}') THEN
                    PERFORM dblink_exec('dbname=postgres', 'CREATE DATABASE {dbName}');
                END IF;
            END
            $$;");
        // NOTE: dblink might not be installed by default. 
        // Simpler way: just try to create and ignore error if it exists, 
        // or check first via a separate command.
    }
    
    // Actually, dblink is overkill. Let's just use a simple check via ExecScriptAsync
    public async Task EnsureDatabaseExistsAsync(string dbName)
    {
        await _postgres.ExecScriptAsync($@"
            SELECT 'CREATE DATABASE {dbName}' 
            WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = '{dbName}')
            \gexec
        ");
    }

    /// <summary>MassTransit-compatible URI e.g. rabbitmq://localhost:PORT</summary>
    public string RabbitMqUri =>
        $"rabbitmq://localhost:{_rabbitMq.GetMappedPublicPort(5672)}";

    public string RabbitMqUsername => "guest";
    public string RabbitMqPassword => "guest";

    // ── Lifecycle ────────────────────────────────────────────────────────────

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            _postgres.StartAsync(),
            _rabbitMq.StartAsync()
        );
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(
            _postgres.DisposeAsync().AsTask(),
            _rabbitMq.DisposeAsync().AsTask()
        );
    }
}
