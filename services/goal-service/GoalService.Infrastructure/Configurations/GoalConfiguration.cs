using GoalService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoalService.Infrastructure.Configurations;

public class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.ToTable("goals");

        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(g => g.Focus)
            .HasColumnName("focus")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(g => g.Object)
            .HasColumnName("object")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(g => g.ActiveFrom)
            .HasColumnName("active_from")
            .IsRequired();

        builder.Property(g => g.ActiveTo)
            .HasColumnName("active_to")
            .IsRequired();

        builder.Property(g => g.Magnitude)
            .HasColumnName("magnitude")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(g => g.Constraints)
            .HasColumnName("constraints")
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(g => g.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(g => g.BaselineProbability)
            .HasColumnName("baseline_probability")
            .HasPrecision(5, 4);

        builder.Property(g => g.DepartmentId)
            .HasColumnName("department_id")
            .IsRequired();

        // Goal 1:N Strategy
        builder.HasMany(g => g.Strategies)
            .WithOne(s => s.Goal)
            .HasForeignKey(s => s.GoalId)
            .OnDelete(DeleteBehavior.Cascade);

        // Goal 0..1 GoalInfluence (a goal can arise from at most one strategy)
        builder.HasOne(g => g.GoalInfluence)
            .WithOne(gi => gi.Goal)
            .HasForeignKey<GoalInfluence>(gi => gi.GoalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
