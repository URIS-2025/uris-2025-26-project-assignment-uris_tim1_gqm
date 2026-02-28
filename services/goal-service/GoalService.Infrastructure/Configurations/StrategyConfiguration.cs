using GoalService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoalService.Infrastructure.Configurations;

public class StrategyConfiguration : IEntityTypeConfiguration<Strategy>
{
    public void Configure(EntityTypeBuilder<Strategy> builder)
    {
        builder.ToTable("strategies");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(s => s.Description)
            .HasColumnName("description")
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(s => s.Effectiveness)
            .HasColumnName("effectiveness")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(s => s.RefinementType)
            .HasColumnName("refinement_type")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(s => s.GoalId)
            .HasColumnName("goal_id")
            .IsRequired();

        // Strategy 1:N GoalInfluence
        builder.HasMany(s => s.GoalInfluences)
            .WithOne(gi => gi.Strategy)
            .HasForeignKey(gi => gi.StrategyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
