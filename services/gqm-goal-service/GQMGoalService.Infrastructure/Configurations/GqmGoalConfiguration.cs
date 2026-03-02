using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GQMGoalService.Domain.Entities;

namespace GQMGoalService.Infrastructure.Configurations;

public class GqmGoalConfiguration : IEntityTypeConfiguration<GqmGoal>
{
    public void Configure(EntityTypeBuilder<GqmGoal> builder)
    {
        builder.ToTable("gqm_goals");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(g => g.CreatedAt)
            .IsRequired();

        builder.Property(g => g.GoalId)
            .IsRequired();

        builder.HasIndex(g => g.GoalId);

        builder.HasMany(g => g.Questions)
            .WithOne(q => q.GqmGoal)
            .HasForeignKey(q => q.GqmGoalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
