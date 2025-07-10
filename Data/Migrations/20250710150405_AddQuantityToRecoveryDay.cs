using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RH.Migrations
{
    /// <inheritdoc />
    public partial class AddQuantityToRecoveryDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Quantity",
                table: "WorkedDaysOff",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "Quantity",
                table: "RecoveryDays",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<float>(
                name: "RecoveryDaysPerWorkedDayOff",
                table: "CompanyInfos",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "WorkedDaysOff");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "RecoveryDays");

            migrationBuilder.DropColumn(
                name: "RecoveryDaysPerWorkedDayOff",
                table: "CompanyInfos");
        }
    }
}
