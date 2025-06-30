using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RH.Migrations
{
    /// <inheritdoc />
    public partial class countableRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "PayrollAppliedRules",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "PayrollAppliedRules",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "PayrollAppliedRules",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCountable",
                table: "PayrollAdjustmentRules",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "PayrollAppliedRules");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "PayrollAppliedRules");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "PayrollAppliedRules");

            migrationBuilder.DropColumn(
                name: "IsCountable",
                table: "PayrollAdjustmentRules");
        }
    }
}
