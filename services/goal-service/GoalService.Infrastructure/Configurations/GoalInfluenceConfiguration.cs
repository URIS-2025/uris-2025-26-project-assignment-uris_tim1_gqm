using GoalService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoalService.Infrastructure.Configurations;

public class GoalInfluenceConfiguration : IEntityTypeConfiguration<GoalInfluence>
{
    public void Configure(EntityTypeBuilder<GoalInfluence> builder)
    {
        builder.ToTable("goal_influences");

        // GoalId is the PK — enforces that a goal can arise from at most one strategy
        builder.HasKey(gi => gi.GoalId);

        builder.Property(gi => gi.GoalId)
            .HasColumnName("goal_id");

        builder.Property(gi => gi.StrategyId)
            .HasColumnName("strategy_id")
            .IsRequired();

        builder.Property(gi => gi.InfluenceType)
            .HasColumnName("influence_type")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(gi => gi.Strength)
            .HasColumnName("strength")
            .HasPrecision(5, 4);

        builder.Property(gi => gi.Confidence)
            .HasColumnName("confidence")
            .HasPrecision(5, 4);

        builder.Property(gi => gi.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(gi => gi.Notes)
            .HasColumnName("notes")
            .HasMaxLength(2000);

        // Index on StrategyId for efficient lookups
        builder.HasIndex(gi => gi.StrategyId);
    }
}
