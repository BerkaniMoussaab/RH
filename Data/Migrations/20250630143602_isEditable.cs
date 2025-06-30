using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RH.Migrations
{
    /// <inheritdoc />
    public partial class isEditable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEditable",
                table: "PayrollAdjustmentRules",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEditable",
                table: "PayrollAdjustmentRules");
        }
    }
}
