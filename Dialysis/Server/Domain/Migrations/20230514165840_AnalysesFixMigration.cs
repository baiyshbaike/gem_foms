using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dialysis.Server.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AnalysesFixMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AnalysisDate",
                table: "FirstAnalysisResult",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AnalysisDate",
                table: "AnalysisResultGroup",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Result",
                table: "AnalysisResult",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AnalysisDate",
                table: "AnalysisResult",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnalysisDate",
                table: "FirstAnalysisResult");

            migrationBuilder.DropColumn(
                name: "AnalysisDate",
                table: "AnalysisResultGroup");

            migrationBuilder.DropColumn(
                name: "AnalysisDate",
                table: "AnalysisResult");

            migrationBuilder.AlterColumn<double>(
                name: "Result",
                table: "AnalysisResult",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
