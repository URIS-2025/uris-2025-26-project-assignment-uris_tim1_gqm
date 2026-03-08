using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveOrganizationIdToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_user_organization_roles_user_id_role_id_organization_id",
                table: "user_organization_roles");

            migrationBuilder.DropColumn(
                name: "organization_id",
                table: "user_organization_roles");

            migrationBuilder.AddColumn<Guid>(
                name: "organization_id",
                table: "users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_organization_id",
                table: "users",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_organization_roles_user_id_role_id",
                table: "user_organization_roles",
                columns: new[] { "user_id", "role_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_organization_id",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_user_organization_roles_user_id_role_id",
                table: "user_organization_roles");

            migrationBuilder.DropColumn(
                name: "organization_id",
                table: "users");

            migrationBuilder.AddColumn<Guid>(
                name: "organization_id",
                table: "user_organization_roles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_organization_roles_user_id_role_id_organization_id",
                table: "user_organization_roles",
                columns: new[] { "user_id", "role_id", "organization_id" },
                unique: true);
        }
    }
}
