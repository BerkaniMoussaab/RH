using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RH.Migrations
{
    /// <inheritdoc />
    public partial class PayrollappliedRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PayrollAppliedRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayrollId = table.Column<int>(type: "int", nullable: false),
                    RuleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollAppliedRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollAppliedRules_PayrollAdjustmentRules_RuleId",
                        column: x => x.RuleId,
                        principalTable: "PayrollAdjustmentRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PayrollAppliedRules_Payrolls_PayrollId",
                        column: x => x.PayrollId,
                        principalTable: "Payrolls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayrollAppliedRules_PayrollId",
                table: "PayrollAppliedRules",
                column: "PayrollId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollAppliedRules_RuleId",
                table: "PayrollAppliedRules",
                column: "RuleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayrollAppliedRules");
        }
    }
}
