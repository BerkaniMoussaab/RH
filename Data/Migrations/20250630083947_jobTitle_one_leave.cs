using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RH.Migrations
{
    /// <inheritdoc />
    public partial class jobTitle_one_leave : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_JobTitles_JobTitleId",
                table: "Employees");

            migrationBuilder.DropTable(
                name: "LeavePolicyJobTitles");

            migrationBuilder.AddColumn<int>(
                name: "LeavePolicyId",
                table: "JobTitles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobTitles_LeavePolicyId",
                table: "JobTitles",
                column: "LeavePolicyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_JobTitles_JobTitleId",
                table: "Employees",
                column: "JobTitleId",
                principalTable: "JobTitles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_JobTitles_LeavePolicies_LeavePolicyId",
                table: "JobTitles",
                column: "LeavePolicyId",
                principalTable: "LeavePolicies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_JobTitles_JobTitleId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_JobTitles_LeavePolicies_LeavePolicyId",
                table: "JobTitles");

            migrationBuilder.DropIndex(
                name: "IX_JobTitles_LeavePolicyId",
                table: "JobTitles");

            migrationBuilder.DropColumn(
                name: "LeavePolicyId",
                table: "JobTitles");

            migrationBuilder.CreateTable(
                name: "LeavePolicyJobTitles",
                columns: table => new
                {
                    JobTitlesId = table.Column<int>(type: "int", nullable: false),
                    LeavePoliciesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeavePolicyJobTitles", x => new { x.JobTitlesId, x.LeavePoliciesId });
                    table.ForeignKey(
                        name: "FK_LeavePolicyJobTitles_JobTitles_JobTitlesId",
                        column: x => x.JobTitlesId,
                        principalTable: "JobTitles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeavePolicyJobTitles_LeavePolicies_LeavePoliciesId",
                        column: x => x.LeavePoliciesId,
                        principalTable: "LeavePolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeavePolicyJobTitles_LeavePoliciesId",
                table: "LeavePolicyJobTitles",
                column: "LeavePoliciesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_JobTitles_JobTitleId",
                table: "Employees",
                column: "JobTitleId",
                principalTable: "JobTitles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
