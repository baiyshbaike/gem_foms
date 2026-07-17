using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UnifyManagerRegionScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ManagerRegionAccesses_Districts_DistrictId",
                table: "ManagerRegionAccesses");

            migrationBuilder.DropForeignKey(
                name: "FK_ManagerRegionAccesses_Regions_RegionId",
                table: "ManagerRegionAccesses");

            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_Regions_RegionId",
                table: "Tenants");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_RegionId",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_ManagerRegionAccesses_DistrictId",
                table: "ManagerRegionAccesses");

            migrationBuilder.DropIndex(
                name: "IX_ManagerRegionAccesses_RegionId",
                table: "ManagerRegionAccesses");

            migrationBuilder.DropIndex(
                name: "IX_ManagerRegionAccesses_UserId",
                table: "ManagerRegionAccesses");

            migrationBuilder.DropIndex(
                name: "IX_ManagerRegionAccesses_UserId_RegionId",
                table: "ManagerRegionAccesses");

            migrationBuilder.DropColumn(
                name: "RegionId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "ManagerRegionAccesses");

            migrationBuilder.DropColumn(
                name: "RegionId",
                table: "ManagerRegionAccesses");

            migrationBuilder.AlterColumn<long>(
                name: "GeoRegionId",
                table: "ManagerRegionAccesses",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ManagerRegionAccesses_UserId_Active",
                table: "ManagerRegionAccesses",
                column: "UserId",
                unique: true,
                filter: "\"RevokedAt\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ManagerRegionAccesses_UserId_Active",
                table: "ManagerRegionAccesses");

            migrationBuilder.AddColumn<string>(
                name: "RegionId",
                table: "Tenants",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "GeoRegionId",
                table: "ManagerRegionAccesses",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "DistrictId",
                table: "ManagerRegionAccesses",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegionId",
                table: "ManagerRegionAccesses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DisabledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_RegionId",
                table: "Tenants",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_ManagerRegionAccesses_DistrictId",
                table: "ManagerRegionAccesses",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_ManagerRegionAccesses_RegionId",
                table: "ManagerRegionAccesses",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_ManagerRegionAccesses_UserId",
                table: "ManagerRegionAccesses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ManagerRegionAccesses_UserId_RegionId",
                table: "ManagerRegionAccesses",
                columns: new[] { "UserId", "RegionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Regions_Code",
                table: "Regions",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ManagerRegionAccesses_Districts_DistrictId",
                table: "ManagerRegionAccesses",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ManagerRegionAccesses_Regions_RegionId",
                table: "ManagerRegionAccesses",
                column: "RegionId",
                principalTable: "Regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_Regions_RegionId",
                table: "Tenants",
                column: "RegionId",
                principalTable: "Regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
