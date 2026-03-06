using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrchestrationService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SagaWorkflows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CurrentStep = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SagaWorkflows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SagaSteps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SagaWorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    StepName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CompensationEndpoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CompensationPayload = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompensatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SagaSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SagaSteps_SagaWorkflows_SagaWorkflowId",
                        column: x => x.SagaWorkflowId,
                        principalTable: "SagaWorkflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SagaWorkflows_GoalId",
                table: "SagaWorkflows",
                column: "GoalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SagaSteps_SagaWorkflowId",
                table: "SagaSteps",
                column: "SagaWorkflowId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "SagaSteps");
            migrationBuilder.DropTable(name: "SagaWorkflows");
        }
    }
}
