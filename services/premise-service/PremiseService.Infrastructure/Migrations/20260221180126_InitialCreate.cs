using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PremiseService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "premises",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    new_version_of_id = table.Column<Guid>(type: "uuid", nullable: true),
                    goal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    strategy_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_premises", x => x.id);
                    table.ForeignKey(
                        name: "FK_premises_premises_new_version_of_id",
                        column: x => x.new_version_of_id,
                        principalTable: "premises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_premises_goal_id",
                table: "premises",
                column: "goal_id");

            migrationBuilder.CreateIndex(
                name: "ix_premises_is_active",
                table: "premises",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_premises_new_version_of_id",
                table: "premises",
                column: "new_version_of_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_premises_strategy_id",
                table: "premises",
                column: "strategy_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "premises");
        }
    }
}
