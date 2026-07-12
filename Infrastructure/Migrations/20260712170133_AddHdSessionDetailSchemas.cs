using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHdSessionDetailSchemas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HdSessionMeasurements",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HdSessionId = table.Column<long>(type: "bigint", nullable: false),
                    Point = table.Column<int>(type: "integer", nullable: false),
                    Sys = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Dia = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Temp = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Ritm = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    MeasuredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HdSessionMeasurements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HdSessionMeasurements_HdSessions_HdSessionId",
                        column: x => x.HdSessionId,
                        principalTable: "HdSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HdSessionPauses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HdSessionId = table.Column<long>(type: "bigint", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PausedBy = table.Column<long>(type: "bigint", nullable: true),
                    ResumedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HdSessionPauses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HdSessionPauses_HdSessions_HdSessionId",
                        column: x => x.HdSessionId,
                        principalTable: "HdSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HdSessionMeasurements_CreatedAt",
                table: "HdSessionMeasurements",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HdSessionMeasurements_HdSessionId_Point",
                table: "HdSessionMeasurements",
                columns: new[] { "HdSessionId", "Point" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HdSessionMeasurements_TenantId",
                table: "HdSessionMeasurements",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_HdSessionPauses_CreatedAt",
                table: "HdSessionPauses",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HdSessionPauses_HdSessionId",
                table: "HdSessionPauses",
                column: "HdSessionId",
                unique: true,
                filter: "\"EndedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HdSessionPauses_TenantId",
                table: "HdSessionPauses",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HdSessionMeasurements");

            migrationBuilder.DropTable(
                name: "HdSessionPauses");
        }
    }
}
