using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DepartmentService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddManagerIdToDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "manager_id",
                table: "departments",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "manager_id",
                table: "departments");
        }
    }
}
