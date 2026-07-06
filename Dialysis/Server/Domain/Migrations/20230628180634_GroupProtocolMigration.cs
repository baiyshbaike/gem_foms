using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Dialysis.Server.Domain.Migrations
{
    /// <inheritdoc />
    public partial class GroupProtocolMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "PatientGroupAct");

            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "PatientGroupAct",
                newName: "PersonTitleId");

            migrationBuilder.AddColumn<string>(
                name: "PersonFio",
                table: "PatientGroupAct",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GroupLPU",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupLPU", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroupPersonTitle",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupPersonTitle", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroupReason",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupReason", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupLPU");

            migrationBuilder.DropTable(
                name: "GroupPersonTitle");

            migrationBuilder.DropTable(
                name: "GroupReason");

            migrationBuilder.DropColumn(
                name: "PersonFio",
                table: "PatientGroupAct");

            migrationBuilder.RenameColumn(
                name: "PersonTitleId",
                table: "PatientGroupAct",
                newName: "PatientId");

            migrationBuilder.AddColumn<long>(
                name: "GroupId",
                table: "PatientGroupAct",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
