using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using OrchestrationService.Domain.Entities;

namespace OrchestrationService.Application.Interfaces.Persistence;

public interface IOrchestrationDbContext
{
    DbSet<SagaWorkflow> SagaWorkflows { get; }
    DbSet<SagaStep> SagaSteps { get; }

    DatabaseFacade Database { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
