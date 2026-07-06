using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Dialysis.Server.Domain.Migrations
{
    /// <inheritdoc />
    public partial class PatientActMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "PatientGroupAct");

            migrationBuilder.DropColumn(
                name: "SignFile",
                table: "PatientGroupAct");

            migrationBuilder.AddColumn<long>(
                name: "GroupTitleId",
                table: "PatientGroupAct",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "Dia",
                table: "HDSession",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EndDia",
                table: "HDSession",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EndRitm",
                table: "HDSession",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EndSys",
                table: "HDSession",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EndTemp",
                table: "HDSession",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ritm",
                table: "HDSession",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sys",
                table: "HDSession",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Temp",
                table: "HDSession",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PatientGroupTitle",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SignFile = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientGroupTitle", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientGroupTitle");

            migrationBuilder.DropColumn(
                name: "GroupTitleId",
                table: "PatientGroupAct");

            migrationBuilder.DropColumn(
                name: "Dia",
                table: "HDSession");

            migrationBuilder.DropColumn(
                name: "EndDia",
                table: "HDSession");

            migrationBuilder.DropColumn(
                name: "EndRitm",
                table: "HDSession");

            migrationBuilder.DropColumn(
                name: "EndSys",
                table: "HDSession");

            migrationBuilder.DropColumn(
                name: "EndTemp",
                table: "HDSession");

            migrationBuilder.DropColumn(
                name: "Ritm",
                table: "HDSession");

            migrationBuilder.DropColumn(
                name: "Sys",
                table: "HDSession");

            migrationBuilder.DropColumn(
                name: "Temp",
                table: "HDSession");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PatientGroupAct",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignFile",
                table: "PatientGroupAct",
                type: "text",
                nullable: true);
        }
    }
}
