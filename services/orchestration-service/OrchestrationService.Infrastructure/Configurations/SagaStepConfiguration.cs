using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrchestrationService.Domain.Entities;

namespace OrchestrationService.Infrastructure.Configurations;

public class SagaStepConfiguration : IEntityTypeConfiguration<SagaStep>
{
    public void Configure(EntityTypeBuilder<SagaStep> builder)
    {
        builder.ToTable("SagaSteps");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SagaWorkflowId)
            .IsRequired();

        builder.Property(s => s.StepName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(s => s.CompensationEndpoint)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.CompensationPayload)
            .HasMaxLength(5000);

        builder.Property(s => s.ExecutedAt)
            .IsRequired();

        builder.Property(s => s.CompensatedAt)
            .IsRequired(false);
    }
}
