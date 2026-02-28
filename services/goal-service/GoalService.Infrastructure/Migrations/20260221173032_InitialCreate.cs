using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoalService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "goals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    focus = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    @object = table.Column<string>(name: "object", type: "character varying(500)", maxLength: 500, nullable: false),
                    active_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    active_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    magnitude = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    constraints = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    baseline_probability = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    department_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goals", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "strategies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    effectiveness = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    refinement_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    goal_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_strategies", x => x.id);
                    table.ForeignKey(
                        name: "FK_strategies_goals_goal_id",
                        column: x => x.goal_id,
                        principalTable: "goals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "goal_influences",
                columns: table => new
                {
                    goal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    strategy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    influence_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    strength = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    confidence = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_goal_influences", x => x.goal_id);
                    table.ForeignKey(
                        name: "FK_goal_influences_goals_goal_id",
                        column: x => x.goal_id,
                        principalTable: "goals",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_goal_influences_strategies_strategy_id",
                        column: x => x.strategy_id,
                        principalTable: "strategies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_goal_influences_strategy_id",
                table: "goal_influences",
                column: "strategy_id");

            migrationBuilder.CreateIndex(
                name: "IX_strategies_goal_id",
                table: "strategies",
                column: "goal_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "goal_influences");

            migrationBuilder.DropTable(
                name: "strategies");

            migrationBuilder.DropTable(
                name: "goals");
        }
    }
}
