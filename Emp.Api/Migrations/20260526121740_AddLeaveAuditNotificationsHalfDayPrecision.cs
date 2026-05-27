using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Emp.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaveAuditNotificationsHalfDayPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DecidedAt",
                table: "Leaves",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DecidedById",
                table: "Leaves",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DecisionNote",
                table: "Leaves",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHalfDay",
                table: "Leaves",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipientEmployeeId = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropColumn(
                name: "DecidedAt",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "DecidedById",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "DecisionNote",
                table: "Leaves");

            migrationBuilder.DropColumn(
                name: "IsHalfDay",
                table: "Leaves");
        }
    }
}
