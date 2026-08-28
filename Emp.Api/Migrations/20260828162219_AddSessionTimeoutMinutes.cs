using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Emp.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionTimeoutMinutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 30 rather than the generated 0 so an existing installation gets the same
            // behaviour as a fresh one (CompanySettings.SessionTimeoutMinutes defaults to 30).
            // An admin can set it back to 0 on the Settings screen to switch the check off.
            migrationBuilder.AddColumn<int>(
                name: "SessionTimeoutMinutes",
                table: "CompanySettings",
                type: "int",
                nullable: false,
                defaultValue: 30);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionTimeoutMinutes",
                table: "CompanySettings");
        }
    }
}
