using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Emp.Api.Migrations
{
    /// <inheritdoc />
    public partial class addBseModelToJobTitleClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "JobTitles",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "JobTitles",
                newName: "UpdatedAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "JobTitles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "JobTitles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "JobTitles",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "JobTitles");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "JobTitles");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "JobTitles");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "JobTitles",
                newName: "CreatedDate");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "JobTitles",
                newName: "IsActive");
        }
    }
}
