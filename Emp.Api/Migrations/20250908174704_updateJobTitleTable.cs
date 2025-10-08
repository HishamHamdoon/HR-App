using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Emp.Api.Migrations
{
    /// <inheritdoc />
    public partial class updateJobTitleTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "JobTitles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MainSalary",
                table: "JobTitles",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "JobTitles");

            migrationBuilder.DropColumn(
                name: "MainSalary",
                table: "JobTitles");
        }
    }
}
