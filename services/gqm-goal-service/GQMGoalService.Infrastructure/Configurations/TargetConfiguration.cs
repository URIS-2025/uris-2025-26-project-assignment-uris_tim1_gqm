using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GQMGoalService.Domain.Entities;
using GQMGoalService.Domain.Enums;

namespace GQMGoalService.Infrastructure.Configurations;

public class TargetConfiguration : IEntityTypeConfiguration<Target>
{
    public void Configure(EntityTypeBuilder<Target> builder)
    {
        builder.ToTable("targets");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        // Store Unit enum as a string for readability in the database
        builder.Property(t => t.Unit)
            .IsRequired()
            .HasConversion(
                v => v.ToString(),
                v => (Unit)Enum.Parse(typeof(Unit), v)
            );

        builder.HasMany(t => t.Measurements)
            .WithOne(m => m.Target)
            .HasForeignKey(m => m.TargetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
