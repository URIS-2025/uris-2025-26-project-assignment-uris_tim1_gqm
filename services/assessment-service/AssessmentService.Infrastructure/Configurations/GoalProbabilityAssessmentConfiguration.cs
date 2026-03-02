using AssessmentService.Domain.Entities;
using AssessmentService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssessmentService.Infrastructure.Configurations;

public class GoalProbabilityAssessmentConfiguration : IEntityTypeConfiguration<GoalProbabilityAssessment>
{
    public void Configure(EntityTypeBuilder<GoalProbabilityAssessment> builder)
    {
        builder.ToTable("goal_probability_assessments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(a => a.GoalId)
            .HasColumnName("goal_id")
            .IsRequired();

        builder.Property(a => a.Probability)
            .HasColumnName("probability")
            .HasPrecision(5, 4)
            .IsRequired();

        builder.Property(a => a.State)
            .HasColumnName("state")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.Method)
            .HasColumnName("method")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.Notes)
            .HasColumnName("notes")
            .HasMaxLength(2000);

        // GoalId is unique — relation 0/1 : 1 (a goal can have at most one assessment)
        builder.HasIndex(a => a.GoalId)
            .IsUnique();
    }
}
