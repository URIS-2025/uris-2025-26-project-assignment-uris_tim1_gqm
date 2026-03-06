using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrchestrationService.Domain.Entities;
using OrchestrationService.Domain.Enums;

namespace OrchestrationService.Infrastructure.Configurations;

public class SagaWorkflowConfiguration : IEntityTypeConfiguration<SagaWorkflow>
{
    public void Configure(EntityTypeBuilder<SagaWorkflow> builder)
    {
        builder.ToTable("SagaWorkflows");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.GoalId)
            .IsRequired();

        builder.HasIndex(w => w.GoalId)
            .IsUnique();

        builder.Property(w => w.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(w => w.CurrentStep)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(w => w.CreatedAt)
            .IsRequired();

        builder.Property(w => w.UpdatedAt)
            .IsRequired();

        builder.HasMany(w => w.Steps)
            .WithOne(s => s.Workflow)
            .HasForeignKey(s => s.SagaWorkflowId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
