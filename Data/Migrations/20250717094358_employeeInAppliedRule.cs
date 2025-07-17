using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RH.Migrations
{
    /// <inheritdoc />
    public partial class employeeInAppliedRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayrollAppliedRules_Payrolls_PayrollId",
                table: "PayrollAppliedRules");

            migrationBuilder.AlterColumn<int>(
                name: "PayrollId",
                table: "PayrollAppliedRules",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "EmployeeId",
                table: "PayrollAppliedRules",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollAppliedRules_EmployeeId",
                table: "PayrollAppliedRules",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PayrollAppliedRules_Employees_EmployeeId",
                table: "PayrollAppliedRules",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PayrollAppliedRules_Payrolls_PayrollId",
                table: "PayrollAppliedRules",
                column: "PayrollId",
                principalTable: "Payrolls",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayrollAppliedRules_Employees_EmployeeId",
                table: "PayrollAppliedRules");

            migrationBuilder.DropForeignKey(
                name: "FK_PayrollAppliedRules_Payrolls_PayrollId",
                table: "PayrollAppliedRules");

            migrationBuilder.DropIndex(
                name: "IX_PayrollAppliedRules_EmployeeId",
                table: "PayrollAppliedRules");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "PayrollAppliedRules");

            migrationBuilder.AlterColumn<int>(
                name: "PayrollId",
                table: "PayrollAppliedRules",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PayrollAppliedRules_Payrolls_PayrollId",
                table: "PayrollAppliedRules",
                column: "PayrollId",
                principalTable: "Payrolls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
