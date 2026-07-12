using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHdSessionCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HdSessions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PatientId = table.Column<long>(type: "bigint", nullable: false),
                    MedCardId = table.Column<long>(type: "bigint", nullable: false),
                    MachineId = table.Column<long>(type: "bigint", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IdentifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FinishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EndIdentifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SentToPayAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PaidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ArchivedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StatusChangedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    StatusReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ActiveMinutes = table.Column<int>(type: "integer", nullable: true),
                    PauseMinutes = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HdSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HdSessions_MedCards_MedCardId",
                        column: x => x.MedCardId,
                        principalTable: "MedCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HdSessions_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SessionWorkflowSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdentificationStartLimitMinutes = table.Column<int>(type: "integer", nullable: false),
                    AutoFinishActiveMinutes = table.Column<int>(type: "integer", nullable: false),
                    EndIdentificationLimitMinutes = table.Column<int>(type: "integer", nullable: false),
                    SendToPayLimitMinutes = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionWorkflowSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HdSessions_CreatedAt",
                table: "HdSessions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HdSessions_MedCardId",
                table: "HdSessions",
                column: "MedCardId");

            migrationBuilder.CreateIndex(
                name: "IX_HdSessions_PatientId",
                table: "HdSessions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_HdSessions_TenantId",
                table: "HdSessions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_HdSessions_TenantId_MachineId",
                table: "HdSessions",
                columns: new[] { "TenantId", "MachineId" });

            migrationBuilder.CreateIndex(
                name: "IX_HdSessions_TenantId_MedCardId",
                table: "HdSessions",
                columns: new[] { "TenantId", "MedCardId" });

            migrationBuilder.CreateIndex(
                name: "IX_HdSessions_TenantId_PatientId",
                table: "HdSessions",
                columns: new[] { "TenantId", "PatientId" });

            migrationBuilder.CreateIndex(
                name: "IX_HdSessions_TenantId_Status",
                table: "HdSessions",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SessionWorkflowSettings_CreatedAt",
                table: "SessionWorkflowSettings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SessionWorkflowSettings_TenantId",
                table: "SessionWorkflowSettings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HdSessions");

            migrationBuilder.DropTable(
                name: "SessionWorkflowSettings");
        }
    }
}
