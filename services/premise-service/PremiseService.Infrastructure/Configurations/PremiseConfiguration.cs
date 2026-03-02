using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PremiseService.Domain.Entities;
using PremiseService.Domain.Enums;

namespace PremiseService.Infrastructure.Configurations;

public class PremiseConfiguration : IEntityTypeConfiguration<Premise>
{
    public void Configure(EntityTypeBuilder<Premise> builder)
    {
        builder.ToTable("premises");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(p => p.Type)
            .HasColumnName("type")
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.NewVersionOfId)
            .HasColumnName("new_version_of_id")
            .IsRequired(false);

        builder.Property(p => p.GoalId)
            .HasColumnName("goal_id")
            .IsRequired(false);

        builder.Property(p => p.StrategyId)
            .HasColumnName("strategy_id")
            .IsRequired(false);

        // Self-referencing relationship for version history (0/1 → 0/1: each premise has at most one newer version)
        builder.HasOne(p => p.NewVersionOf)
            .WithOne(p => p.NewerVersion)
            .HasForeignKey<Premise>(p => p.NewVersionOfId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(p => p.GoalId).HasDatabaseName("ix_premises_goal_id");
        builder.HasIndex(p => p.StrategyId).HasDatabaseName("ix_premises_strategy_id");
        builder.HasIndex(p => p.IsActive).HasDatabaseName("ix_premises_is_active");
        builder.HasIndex(p => p.NewVersionOfId).HasDatabaseName("ix_premises_new_version_of_id");
    }
}
