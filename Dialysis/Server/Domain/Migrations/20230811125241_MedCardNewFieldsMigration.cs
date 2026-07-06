using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dialysis.Server.Domain.Migrations
{
    /// <inheritdoc />
    public partial class MedCardNewFieldsMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Complication1Code",
                table: "MedCard",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Complication1Date",
                table: "MedCard",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Complication2Code",
                table: "MedCard",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Complication2Date",
                table: "MedCard",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Complication3Code",
                table: "MedCard",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Complication3Date",
                table: "MedCard",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Diagnosis1Code",
                table: "MedCard",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Diagnosis1Date",
                table: "MedCard",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Diagnosis2Code",
                table: "MedCard",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Diagnosis2Date",
                table: "MedCard",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Diagnosis3Code",
                table: "MedCard",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Diagnosis3Date",
                table: "MedCard",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MainDiagnosisCode",
                table: "MedCard",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MainDiagnosisDate",
                table: "MedCard",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherABCode",
                table: "MedCard",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OtherABDate",
                table: "MedCard",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OutTreatment",
                table: "MedCard",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Complication1Code",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "Complication1Date",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "Complication2Code",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "Complication2Date",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "Complication3Code",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "Complication3Date",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "Diagnosis1Code",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "Diagnosis1Date",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "Diagnosis2Code",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "Diagnosis2Date",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "Diagnosis3Code",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "Diagnosis3Date",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "MainDiagnosisCode",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "MainDiagnosisDate",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "OtherABCode",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "OtherABDate",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "OutTreatment",
                table: "MedCard");
        }
    }
}
