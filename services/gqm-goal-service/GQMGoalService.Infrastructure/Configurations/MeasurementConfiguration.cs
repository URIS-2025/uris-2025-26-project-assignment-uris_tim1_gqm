using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GQMGoalService.Domain.Entities;

namespace GQMGoalService.Infrastructure.Configurations;

public class MeasurementConfiguration : IEntityTypeConfiguration<Measurement>
{
    public void Configure(EntityTypeBuilder<Measurement> builder)
    {
        builder.ToTable("measurements");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Value)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(m => m.MeasuredAt)
            .IsRequired();
    }
}
