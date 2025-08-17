using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RH.Migrations
{
    /// <inheritdoc />
    public partial class TriggerRemoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Trigger",
                table: "PayrollAdjustmentRules");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Trigger",
                table: "PayrollAdjustmentRules",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
