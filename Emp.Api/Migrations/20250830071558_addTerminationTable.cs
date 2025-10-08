using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Emp.Api.Migrations
{
    /// <inheritdoc />
    public partial class addTerminationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Terminations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    TerminationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TerminationReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateTerminated = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Terminations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Terminations_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Terminations_EmployeeId",
                table: "Terminations",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Terminations");
        }
    }
}
