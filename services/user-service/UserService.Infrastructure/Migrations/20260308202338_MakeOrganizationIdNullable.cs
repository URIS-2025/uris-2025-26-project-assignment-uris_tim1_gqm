using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeOrganizationIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_user_organization_roles",
                table: "user_organization_roles");

            migrationBuilder.DropIndex(
                name: "IX_user_organization_roles_organization_id",
                table: "user_organization_roles");

            migrationBuilder.AlterColumn<Guid>(
                name: "organization_id",
                table: "user_organization_roles",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "id",
                table: "user_organization_roles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_organization_roles",
                table: "user_organization_roles",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_user_organization_roles_user_id_role_id_organization_id",
                table: "user_organization_roles",
                columns: new[] { "user_id", "role_id", "organization_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_user_organization_roles",
                table: "user_organization_roles");

            migrationBuilder.DropIndex(
                name: "IX_user_organization_roles_user_id_role_id_organization_id",
                table: "user_organization_roles");

            migrationBuilder.DropColumn(
                name: "id",
                table: "user_organization_roles");

            migrationBuilder.AlterColumn<Guid>(
                name: "organization_id",
                table: "user_organization_roles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_organization_roles",
                table: "user_organization_roles",
                columns: new[] { "user_id", "role_id", "organization_id" });

            migrationBuilder.CreateIndex(
                name: "IX_user_organization_roles_organization_id",
                table: "user_organization_roles",
                column: "organization_id");
        }
    }
}
