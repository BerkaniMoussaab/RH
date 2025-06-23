using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RH.Migrations
{
    /// <inheritdoc />
    public partial class AbsencePayroll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeductionValue",
                table: "AbsenceRecords");

            migrationBuilder.AddColumn<int>(
                name: "PayrollId",
                table: "AbsenceRecords",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayrollId",
                table: "AbsenceRecords");

            migrationBuilder.AddColumn<int>(
                name: "DeductionValue",
                table: "AbsenceRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
