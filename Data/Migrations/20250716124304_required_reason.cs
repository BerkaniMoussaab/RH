using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RH.Migrations
{
    /// <inheritdoc />
    public partial class required_reason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdvanceDeductions_Advances_AdvanceId",
                table: "AdvanceDeductions");

            migrationBuilder.DropForeignKey(
                name: "FK_AdvanceDeductions_Payrolls_PayrollId",
                table: "AdvanceDeductions");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "Advances",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Advances_Date",
                table: "Advances",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Advances_Status",
                table: "Advances",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AdvanceDeductions_DeductionDate",
                table: "AdvanceDeductions",
                column: "DeductionDate");

            migrationBuilder.AddForeignKey(
                name: "FK_AdvanceDeductions_Advances_AdvanceId",
                table: "AdvanceDeductions",
                column: "AdvanceId",
                principalTable: "Advances",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AdvanceDeductions_Payrolls_PayrollId",
                table: "AdvanceDeductions",
                column: "PayrollId",
                principalTable: "Payrolls",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdvanceDeductions_Advances_AdvanceId",
                table: "AdvanceDeductions");

            migrationBuilder.DropForeignKey(
                name: "FK_AdvanceDeductions_Payrolls_PayrollId",
                table: "AdvanceDeductions");

            migrationBuilder.DropIndex(
                name: "IX_Advances_Date",
                table: "Advances");

            migrationBuilder.DropIndex(
                name: "IX_Advances_Status",
                table: "Advances");

            migrationBuilder.DropIndex(
                name: "IX_AdvanceDeductions_DeductionDate",
                table: "AdvanceDeductions");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "Advances",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddForeignKey(
                name: "FK_AdvanceDeductions_Advances_AdvanceId",
                table: "AdvanceDeductions",
                column: "AdvanceId",
                principalTable: "Advances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AdvanceDeductions_Payrolls_PayrollId",
                table: "AdvanceDeductions",
                column: "PayrollId",
                principalTable: "Payrolls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
