using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RH.Migrations
{
    public partial class BaseSalary_Employe : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BaseSalary",
                table: "Employees",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            // Set BaseSalary from latest payroll or 0 if not found
            migrationBuilder.Sql(@"
                UPDATE Employees
                SET BaseSalary = ISNULL((
                    SELECT TOP 1 p.BaseSalary
                    FROM Payrolls p
                    WHERE p.EmployeeId = Employees.Id
                    ORDER BY p.Id DESC
                ), 0)
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseSalary",
                table: "Employees");
        }
    }
}