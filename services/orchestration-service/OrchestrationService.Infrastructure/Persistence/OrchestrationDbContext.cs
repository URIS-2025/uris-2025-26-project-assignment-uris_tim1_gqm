using Microsoft.EntityFrameworkCore;
using OrchestrationService.Application.Interfaces.Persistence;
using OrchestrationService.Domain.Entities;
using MassTransit;

namespace OrchestrationService.Infrastructure.Persistence;

public class OrchestrationDbContext : DbContext, IOrchestrationDbContext
{
    public OrchestrationDbContext(DbContextOptions<OrchestrationDbContext> options) : base(options) { }

    public DbSet<SagaWorkflow> SagaWorkflows => Set<SagaWorkflow>();
    public DbSet<SagaStep> SagaSteps => Set<SagaStep>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrchestrationDbContext).Assembly);
    }
}
