using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RH.Migrations
{
    /// <inheritdoc />
    public partial class relationPayrollAbsence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AbsenceRecords_PayrollId",
                table: "AbsenceRecords",
                column: "PayrollId");

            migrationBuilder.AddForeignKey(
                name: "FK_AbsenceRecords_Payrolls_PayrollId",
                table: "AbsenceRecords",
                column: "PayrollId",
                principalTable: "Payrolls",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AbsenceRecords_Payrolls_PayrollId",
                table: "AbsenceRecords");

            migrationBuilder.DropIndex(
                name: "IX_AbsenceRecords_PayrollId",
                table: "AbsenceRecords");
        }
    }
}
