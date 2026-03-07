using DepartmentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DepartmentService.Infrastructure.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(d => d.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(d => d.OrganizationId)
            .HasColumnName("organization_id")
            .IsRequired();

        builder.Property(d => d.ManagerId)
            .HasColumnName("manager_id")
            .IsRequired(false);

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasOne(d => d.Organization)
            .WithMany(o => o.Departments)
            .HasForeignKey(d => d.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => d.OrganizationId);

        builder.HasIndex(d => new { d.OrganizationId, d.Name })
            .IsUnique();
    }
}
