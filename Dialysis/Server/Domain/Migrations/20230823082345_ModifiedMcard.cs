using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Dialysis.Server.Domain.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedMcard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "Complication1Time",
                table: "MedCard",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Complication2Time",
                table: "MedCard",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Complication3Time",
                table: "MedCard",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Diagnosis1Time",
                table: "MedCard",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Diagnosis2Time",
                table: "MedCard",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Diagnosis3Time",
                table: "MedCard",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "DirectionTime",
                table: "MedCard",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FIODepartmentHead",
                table: "MedCard",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FIODoctor",
                table: "MedCard",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Familiarized",
                table: "MedCard",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "MainDiagnosisTime",
                table: "MedCard",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "OtherABTime",
                table: "MedCard",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PassportNum",
                table: "MedCard",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ReceiptTime",
                table: "MedCard",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusOfArteriovenous",
                table: "FirstNeuro",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bedsores",
                table: "FirstInspection",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BodyT",
                table: "FirstInspection",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Complaints",
                table: "FirstInspection",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Damage",
                table: "FirstInspection",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeformationOfBones",
                table: "FirstInspection",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Dermographism",
                table: "FirstInspection",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeneralState",
                table: "FirstInspection",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Musculature",
                table: "FirstInspection",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Other",
                table: "FirstInspection",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PainMuscles",
                table: "FirstInspection",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pigmentation",
                table: "FirstInspection",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Scars",
                table: "FirstInspection",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "КednessOfJoints",
                table: "FirstInspection",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Prochee2",
                table: "FirstConfectionery",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Prochee5",
                table: "FirstCardiovascular",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CodeMKBs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    AgeProperty = table.Column<string>(type: "text", nullable: true),
                    Pol = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    GlobalStatusId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CodeMKBs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CodeMKBs");

            migrationBuilder.DropColumn(
                name: "Complication1Time",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "Complication2Time",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "Complication3Time",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "Diagnosis1Time",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "Diagnosis2Time",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "Diagnosis3Time",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "DirectionTime",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "FIODepartmentHead",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "FIODoctor",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "Familiarized",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "MainDiagnosisTime",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "OtherABTime",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "PassportNum",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "ReceiptTime",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "StatusOfArteriovenous",
                table: "FirstNeuro");

            migrationBuilder.DropColumn(
                name: "Bedsores",
                table: "FirstInspection");

            migrationBuilder.DropColumn(
                name: "BodyT",
                table: "FirstInspection");

            migrationBuilder.DropColumn(
                name: "Complaints",
                table: "FirstInspection");

            migrationBuilder.DropColumn(
                name: "Damage",
                table: "FirstInspection");

            migrationBuilder.DropColumn(
                name: "DeformationOfBones",
                table: "FirstInspection");

            migrationBuilder.DropColumn(
                name: "Dermographism",
                table: "FirstInspection");

            migrationBuilder.DropColumn(
                name: "GeneralState",
                table: "FirstInspection");

            migrationBuilder.DropColumn(
                name: "Musculature",
                table: "FirstInspection");

            migrationBuilder.DropColumn(
                name: "Other",
                table: "FirstInspection");

            migrationBuilder.DropColumn(
                name: "PainMuscles",
                table: "FirstInspection");

            migrationBuilder.DropColumn(
                name: "Pigmentation",
                table: "FirstInspection");

            migrationBuilder.DropColumn(
                name: "Scars",
                table: "FirstInspection");

            migrationBuilder.DropColumn(
                name: "КednessOfJoints",
                table: "FirstInspection");

            migrationBuilder.DropColumn(
                name: "Prochee2",
                table: "FirstConfectionery");

            migrationBuilder.DropColumn(
                name: "Prochee5",
                table: "FirstCardiovascular");
        }
    }
}
