using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RH.Migrations
{
    /// <inheritdoc />
    public partial class AdjustmentRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayrollAdjustmentRules_JobTitles_JobTitleId",
                table: "PayrollAdjustmentRules");

            migrationBuilder.DropIndex(
                name: "IX_PayrollAdjustmentRules_JobTitleId",
                table: "PayrollAdjustmentRules");

            migrationBuilder.DropColumn(
                name: "JobTitleId",
                table: "PayrollAdjustmentRules");

            migrationBuilder.CreateTable(
                name: "JobTitlePayrollAdjustmentRule",
                columns: table => new
                {
                    JobTitlesId = table.Column<int>(type: "int", nullable: false),
                    PayrollAdjustmentRulesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobTitlePayrollAdjustmentRule", x => new { x.JobTitlesId, x.PayrollAdjustmentRulesId });
                    table.ForeignKey(
                        name: "FK_JobTitlePayrollAdjustmentRule_JobTitles_JobTitlesId",
                        column: x => x.JobTitlesId,
                        principalTable: "JobTitles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobTitlePayrollAdjustmentRule_PayrollAdjustmentRules_PayrollAdjustmentRulesId",
                        column: x => x.PayrollAdjustmentRulesId,
                        principalTable: "PayrollAdjustmentRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobTitlePayrollAdjustmentRule_PayrollAdjustmentRulesId",
                table: "JobTitlePayrollAdjustmentRule",
                column: "PayrollAdjustmentRulesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobTitlePayrollAdjustmentRule");

            migrationBuilder.AddColumn<int>(
                name: "JobTitleId",
                table: "PayrollAdjustmentRules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollAdjustmentRules_JobTitleId",
                table: "PayrollAdjustmentRules",
                column: "JobTitleId");

            migrationBuilder.AddForeignKey(
                name: "FK_PayrollAdjustmentRules_JobTitles_JobTitleId",
                table: "PayrollAdjustmentRules",
                column: "JobTitleId",
                principalTable: "JobTitles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
