using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantTimeZoneId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HdSessions_TenantId_MachineId",
                table: "HdSessions");

            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                table: "Tenants",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MedCenterMachines",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AcquisitionType = table.Column<int>(type: "integer", nullable: false),
                    InventoryNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SerialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Manufacturer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ManufacturingCountry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ManufactureYear = table.Column<int>(type: "integer", nullable: false),
                    CertificateHolder = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CertificateHolderCountry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CertificateNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CertificateCountry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CertificateIssuedAt = table.Column<DateOnly>(type: "date", nullable: false),
                    PermitName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PermitNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PermitSeries = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PermitExpiresAt = table.Column<DateOnly>(type: "date", nullable: false),
                    DailySessionLimit = table.Column<int>(type: "integer", nullable: false),
                    BetweenSessionCooldownMinutes = table.Column<int>(type: "integer", nullable: false),
                    DailyLimitCooldownMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedCenterMachines", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HdSessions_MachineId",
                table: "HdSessions",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "UX_HdSessions_ActiveMachine",
                table: "HdSessions",
                columns: new[] { "TenantId", "MachineId" },
                unique: true,
                filter: "\"MachineId\" IS NOT NULL AND \"Status\" IN (2, 3)");

            migrationBuilder.CreateIndex(
                name: "IX_MedCenterMachines_CreatedAt",
                table: "MedCenterMachines",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MedCenterMachines_IsActive",
                table: "MedCenterMachines",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MedCenterMachines_IsDeleted",
                table: "MedCenterMachines",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_MedCenterMachines_PermitExpiresAt",
                table: "MedCenterMachines",
                column: "PermitExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_MedCenterMachines_SerialNumber",
                table: "MedCenterMachines",
                column: "SerialNumber",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MedCenterMachines_TenantId",
                table: "MedCenterMachines",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_MedCenterMachines_TenantId_InventoryNumber",
                table: "MedCenterMachines",
                columns: new[] { "TenantId", "InventoryNumber" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MedCenterMachines_TenantId_IsActive",
                table: "MedCenterMachines",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MedCenterMachines_TenantId_IsApproved",
                table: "MedCenterMachines",
                columns: new[] { "TenantId", "IsApproved" });

            migrationBuilder.AddForeignKey(
                name: "FK_HdSessions_MedCenterMachines_MachineId",
                table: "HdSessions",
                column: "MachineId",
                principalTable: "MedCenterMachines",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HdSessions_MedCenterMachines_MachineId",
                table: "HdSessions");

            migrationBuilder.DropTable(
                name: "MedCenterMachines");

            migrationBuilder.DropIndex(
                name: "IX_HdSessions_MachineId",
                table: "HdSessions");

            migrationBuilder.DropIndex(
                name: "UX_HdSessions_ActiveMachine",
                table: "HdSessions");

            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                table: "Tenants");

            migrationBuilder.CreateIndex(
                name: "IX_HdSessions_TenantId_MachineId",
                table: "HdSessions",
                columns: new[] { "TenantId", "MachineId" });
        }
    }
}
