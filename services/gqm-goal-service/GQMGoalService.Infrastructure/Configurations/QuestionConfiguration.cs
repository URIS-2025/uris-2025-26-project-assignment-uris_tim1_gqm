using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GQMGoalService.Domain.Entities;

namespace GQMGoalService.Infrastructure.Configurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("questions");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Text)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(q => q.CreatedAt)
            .IsRequired();

        builder.HasMany(q => q.Targets)
            .WithOne(t => t.Question)
            .HasForeignKey(t => t.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
