using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RH.Migrations
{
    /// <inheritdoc />
    public partial class company_employeeInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "BirthDate",
                table: "Employees",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BirthPlace",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "CompanyInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DirectorName",
                table: "CompanyInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DirectorTitle",
                table: "CompanyInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "CompanyInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Mobile",
                table: "CompanyInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "CompanyInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                table: "CompanyInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "BirthPlace",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "City",
                table: "CompanyInfos");

            migrationBuilder.DropColumn(
                name: "DirectorName",
                table: "CompanyInfos");

            migrationBuilder.DropColumn(
                name: "DirectorTitle",
                table: "CompanyInfos");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "CompanyInfos");

            migrationBuilder.DropColumn(
                name: "Mobile",
                table: "CompanyInfos");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "CompanyInfos");

            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                table: "CompanyInfos");
        }
    }
}
