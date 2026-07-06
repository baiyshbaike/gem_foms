using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dialysis.Server.Domain.Migrations
{
    /// <inheritdoc />
    public partial class MachineFixMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Country",
                table: "MedCenterMachine",
                newName: "UpFile3");

            migrationBuilder.AddColumn<string>(
                name: "GroupText",
                table: "Patient",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CertificateHolder",
                table: "MedCenterMachine",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CertificateHolderCountry",
                table: "MedCenterMachine",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManCountry",
                table: "MedCenterMachine",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ManDate",
                table: "MedCenterMachine",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PermitDate",
                table: "MedCenterMachine",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PermitName",
                table: "MedCenterMachine",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PermitNumber",
                table: "MedCenterMachine",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PermitSeria",
                table: "MedCenterMachine",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpFile1",
                table: "MedCenterMachine",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpFile2",
                table: "MedCenterMachine",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "NextDays",
                table: "Analysise",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupText",
                table: "Patient");

            migrationBuilder.DropColumn(
                name: "CertificateHolder",
                table: "MedCenterMachine");

            migrationBuilder.DropColumn(
                name: "CertificateHolderCountry",
                table: "MedCenterMachine");

            migrationBuilder.DropColumn(
                name: "ManCountry",
                table: "MedCenterMachine");

            migrationBuilder.DropColumn(
                name: "ManDate",
                table: "MedCenterMachine");

            migrationBuilder.DropColumn(
                name: "PermitDate",
                table: "MedCenterMachine");

            migrationBuilder.DropColumn(
                name: "PermitName",
                table: "MedCenterMachine");

            migrationBuilder.DropColumn(
                name: "PermitNumber",
                table: "MedCenterMachine");

            migrationBuilder.DropColumn(
                name: "PermitSeria",
                table: "MedCenterMachine");

            migrationBuilder.DropColumn(
                name: "UpFile1",
                table: "MedCenterMachine");

            migrationBuilder.DropColumn(
                name: "UpFile2",
                table: "MedCenterMachine");

            migrationBuilder.DropColumn(
                name: "NextDays",
                table: "Analysise");

            migrationBuilder.RenameColumn(
                name: "UpFile3",
                table: "MedCenterMachine",
                newName: "Country");
        }
    }
}
