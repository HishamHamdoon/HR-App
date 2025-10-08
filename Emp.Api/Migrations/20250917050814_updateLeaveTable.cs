using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Emp.Api.Migrations
{
    /// <inheritdoc />
    public partial class updateLeaveTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leaves_Employees_ManagerId",
                table: "Leaves");

            migrationBuilder.CreateIndex(
                name: "IX_Leaves_EmployeeId",
                table: "Leaves",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Leaves_Employees_EmployeeId",
                table: "Leaves",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Leaves_Employees_ManagerId",
                table: "Leaves",
                column: "ManagerId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Leaves_Employees_EmployeeId",
                table: "Leaves");

            migrationBuilder.DropForeignKey(
                name: "FK_Leaves_Employees_ManagerId",
                table: "Leaves");

            migrationBuilder.DropIndex(
                name: "IX_Leaves_EmployeeId",
                table: "Leaves");

            migrationBuilder.AddForeignKey(
                name: "FK_Leaves_Employees_ManagerId",
                table: "Leaves",
                column: "ManagerId",
                principalTable: "Employees",
                principalColumn: "Id");
        }
    }
}
