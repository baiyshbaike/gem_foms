using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Dialysis.Server.Domain.Migrations
{
    /// <inheritdoc />
    public partial class MedCardFixMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UniqId",
                table: "MedCard",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UniqId",
                table: "FirstUrogenital",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UniqId",
                table: "FirstRespiratory",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UniqId",
                table: "FirstNeuro",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UniqId",
                table: "FirstInspection",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UniqId",
                table: "FirstEndocrine",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UniqId",
                table: "FirstConfectionery",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UniqId",
                table: "FirstCardiovascular",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UniqId",
                table: "FirstAnalysisResult",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UniqId",
                table: "FirstAnalysis",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PatientHistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PatientId = table.Column<long>(type: "bigint", nullable: false),
                    GroupId = table.Column<long>(type: "bigint", nullable: false),
                    GroupTitleId = table.Column<long>(type: "bigint", nullable: false),
                    GroupFromId = table.Column<long>(type: "bigint", nullable: true),
                    GroupText = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ActDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UniqID = table.Column<string>(type: "text", nullable: true),
                    GroupReasonId = table.Column<long>(type: "bigint", nullable: true),
                    GroupLPUId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientHistory", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientHistory");

            migrationBuilder.DropColumn(
                name: "UniqId",
                table: "MedCard");

            migrationBuilder.DropColumn(
                name: "UniqId",
                table: "FirstUrogenital");

            migrationBuilder.DropColumn(
                name: "UniqId",
                table: "FirstRespiratory");

            migrationBuilder.DropColumn(
                name: "UniqId",
                table: "FirstNeuro");

            migrationBuilder.DropColumn(
                name: "UniqId",
                table: "FirstInspection");

            migrationBuilder.DropColumn(
                name: "UniqId",
                table: "FirstEndocrine");

            migrationBuilder.DropColumn(
                name: "UniqId",
                table: "FirstConfectionery");

            migrationBuilder.DropColumn(
                name: "UniqId",
                table: "FirstCardiovascular");

            migrationBuilder.DropColumn(
                name: "UniqId",
                table: "FirstAnalysisResult");

            migrationBuilder.DropColumn(
                name: "UniqId",
                table: "FirstAnalysis");
        }
    }
}
