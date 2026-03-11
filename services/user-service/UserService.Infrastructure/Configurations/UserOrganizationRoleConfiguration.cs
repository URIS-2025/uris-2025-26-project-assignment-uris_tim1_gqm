using UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UserService.Infrastructure.Configurations;

public class UserOrganizationRoleConfiguration : IEntityTypeConfiguration<UserOrganizationRole>
{
    public void Configure(EntityTypeBuilder<UserOrganizationRole> builder)
    {
        builder.ToTable("user_organization_roles");

        builder.HasKey(uor => uor.Id);

        builder.Property(uor => uor.Id)
            .HasColumnName("id");

        builder.Property(uor => uor.UserId)
            .HasColumnName("user_id");

        builder.Property(uor => uor.RoleId)
            .HasColumnName("role_id");

        builder.HasOne(uor => uor.User)
            .WithMany(u => u.UserOrganizationRoles)
            .HasForeignKey(uor => uor.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(uor => uor.Role)
            .WithMany(r => r.UserOrganizationRoles)
            .HasForeignKey(uor => uor.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(uor => new { uor.UserId, uor.RoleId })
            .IsUnique();
    }
}
