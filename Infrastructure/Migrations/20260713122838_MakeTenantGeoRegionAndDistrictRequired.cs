using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeTenantGeoRegionAndDistrictRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "Tenants" AS t
                SET
                    "DistrictId" = COALESCE(t."DistrictId", d."Id"),
                    "GeoRegionId" = COALESCE(t."GeoRegionId", d."RegionId")
                FROM (
                    SELECT "Id", "RegionId"
                    FROM "Districts"
                    WHERE "IsActive" = TRUE AND "IsDeleted" = FALSE
                    ORDER BY "Id"
                    LIMIT 1
                ) AS d
                WHERE t."DistrictId" IS NULL OR t."GeoRegionId" IS NULL;
                """);

            migrationBuilder.AlterColumn<long>(
                name: "GeoRegionId",
                table: "Tenants",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "DistrictId",
                table: "Tenants",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "GeoRegionId",
                table: "Tenants",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "DistrictId",
                table: "Tenants",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
