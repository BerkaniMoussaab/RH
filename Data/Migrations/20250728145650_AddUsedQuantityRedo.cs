using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RH.Migrations
{
    /// <inheritdoc />
    public partial class AddUsedQuantityRedo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Used",
                table: "RecoveryDays");

            migrationBuilder.CreateTable(
                name: "RecoveryDayUsages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecoveryDayId = table.Column<int>(type: "int", nullable: false),
                    UsageDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuantityUsed = table.Column<float>(type: "real", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecoveryDayUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecoveryDayUsages_RecoveryDays_RecoveryDayId",
                        column: x => x.RecoveryDayId,
                        principalTable: "RecoveryDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecoveryDayUsages_RecoveryDayId",
                table: "RecoveryDayUsages",
                column: "RecoveryDayId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecoveryDayUsages");

            migrationBuilder.AddColumn<bool>(
                name: "Used",
                table: "RecoveryDays",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
