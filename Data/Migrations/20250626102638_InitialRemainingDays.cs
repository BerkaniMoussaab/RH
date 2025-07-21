using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RH.Migrations
{
    /// <inheritdoc />
    public partial class InitialRemainingDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ConsumedLeaveDaysThisYear",
                table: "Employees",
                newName: "InitialRemainingDays");

            migrationBuilder.AddColumn<DateTime>(
                name: "InscriptionDate",
                table: "Employees",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InscriptionDate",
                table: "Employees");

            migrationBuilder.RenameColumn(
                name: "InitialRemainingDays",
                table: "Employees",
                newName: "ConsumedLeaveDaysThisYear");
        }
    }
}
