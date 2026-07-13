using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGeoRegionsAndDistricts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DistrictId",
                table: "Tenants",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "GeoRegionId",
                table: "Tenants",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DistrictId",
                table: "ManagerRegionAccesses",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "GeoRegionId",
                table: "ManagerRegionAccesses",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GeoRegions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoRegions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RegionId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Districts_GeoRegions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "GeoRegions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "GeoRegions",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "IsActive", "IsDeleted", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Баткенская область", null, null },
                    { 2L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Чуйская область", null, null },
                    { 3L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Иссык-Кульская область", null, null },
                    { 4L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Джалал-Абадская область", null, null },
                    { 5L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Нарынская область", null, null },
                    { 6L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Ошская область", null, null },
                    { 7L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Таласская область", null, null },
                    { 8L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Город Бишкек", null, null },
                    { 9L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Город Ош", null, null }
                });

            migrationBuilder.InsertData(
                table: "Districts",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "DeletedAt", "IsActive", "IsDeleted", "Name", "RegionId", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Баткенский район", 1L, null, null },
                    { 2L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Кадамжайский район", 1L, null, null },
                    { 3L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Лейлекский район", 1L, null, null },
                    { 4L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Аламудунский район", 2L, null, null },
                    { 5L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Чуйский район", 2L, null, null },
                    { 6L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Жайылский район", 2L, null, null },
                    { 7L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Кеминский район", 2L, null, null },
                    { 8L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Московский район", 2L, null, null },
                    { 9L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Панфиловский район", 2L, null, null },
                    { 10L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Сокулукский район", 2L, null, null },
                    { 11L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Ысык-Атинский район", 2L, null, null },
                    { 12L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Ак-Суйский район", 3L, null, null },
                    { 13L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Иссык-Кульский район", 3L, null, null },
                    { 14L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Джети-Огузский район", 3L, null, null },
                    { 15L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Тонский район", 3L, null, null },
                    { 16L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Тюпский район", 3L, null, null },
                    { 17L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Аксыйский район", 4L, null, null },
                    { 18L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Ала-Букинский район", 4L, null, null },
                    { 19L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Базар-Коргонский район", 4L, null, null },
                    { 20L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Чаткальский район", 4L, null, null },
                    { 21L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Ноокенский район", 4L, null, null },
                    { 22L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Сузакский район", 4L, null, null },
                    { 23L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Токтогульский район", 4L, null, null },
                    { 24L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Тогуз-Тороуский район", 4L, null, null },
                    { 25L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Ак-Талинский район", 5L, null, null },
                    { 26L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Ат-Башинский район", 5L, null, null },
                    { 27L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Жумгальский район", 5L, null, null },
                    { 28L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Кочкорский район", 5L, null, null },
                    { 29L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Нарынский район", 5L, null, null },
                    { 30L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Алайский район", 6L, null, null },
                    { 31L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Араванский район", 6L, null, null },
                    { 32L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Чон-Алайский район", 6L, null, null },
                    { 33L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Кара-Кульджинский район", 6L, null, null },
                    { 34L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Кара-Сууский район", 6L, null, null },
                    { 35L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Ноокатский район", 6L, null, null },
                    { 36L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Узгенский район", 6L, null, null },
                    { 37L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Айтматовский район", 7L, null, null },
                    { 38L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Бакай-Атинский район", 7L, null, null },
                    { 39L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Манасский район", 7L, null, null },
                    { 40L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Таласский район", 7L, null, null },
                    { 41L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Первомайский район", 8L, null, null },
                    { 42L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Ленинский район", 8L, null, null },
                    { 43L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Октябрьский район", 8L, null, null },
                    { 44L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Свердловский район", 8L, null, null },
                    { 45L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Амир-Тимур", 9L, null, null },
                    { 46L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Керме-Тоо", 9L, null, null },
                    { 47L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Курманжан-Датка", 9L, null, null },
                    { 48L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Манас-Ата", 9L, null, null },
                    { 49L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Сулайман-Тоо", 9L, null, null },
                    { 50L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 0L, null, true, false, "Туран", 9L, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_DistrictId",
                table: "Tenants",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_GeoRegionId",
                table: "Tenants",
                column: "GeoRegionId");

            migrationBuilder.CreateIndex(
                name: "IX_ManagerRegionAccesses_DistrictId",
                table: "ManagerRegionAccesses",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_ManagerRegionAccesses_GeoRegionId",
                table: "ManagerRegionAccesses",
                column: "GeoRegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_CreatedAt",
                table: "Districts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_IsActive",
                table: "Districts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_IsDeleted",
                table: "Districts",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_RegionId",
                table: "Districts",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_RegionId_Name",
                table: "Districts",
                columns: new[] { "RegionId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_GeoRegions_CreatedAt",
                table: "GeoRegions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_GeoRegions_IsActive",
                table: "GeoRegions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_GeoRegions_IsDeleted",
                table: "GeoRegions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_GeoRegions_Name",
                table: "GeoRegions",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.AddForeignKey(
                name: "FK_ManagerRegionAccesses_Districts_DistrictId",
                table: "ManagerRegionAccesses",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ManagerRegionAccesses_GeoRegions_GeoRegionId",
                table: "ManagerRegionAccesses",
                column: "GeoRegionId",
                principalTable: "GeoRegions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_Districts_DistrictId",
                table: "Patients",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_GeoRegions_RegionId",
                table: "Patients",
                column: "RegionId",
                principalTable: "GeoRegions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_Districts_DistrictId",
                table: "Tenants",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tenants_GeoRegions_GeoRegionId",
                table: "Tenants",
                column: "GeoRegionId",
                principalTable: "GeoRegions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ManagerRegionAccesses_Districts_DistrictId",
                table: "ManagerRegionAccesses");

            migrationBuilder.DropForeignKey(
                name: "FK_ManagerRegionAccesses_GeoRegions_GeoRegionId",
                table: "ManagerRegionAccesses");

            migrationBuilder.DropForeignKey(
                name: "FK_Patients_Districts_DistrictId",
                table: "Patients");

            migrationBuilder.DropForeignKey(
                name: "FK_Patients_GeoRegions_RegionId",
                table: "Patients");

            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_Districts_DistrictId",
                table: "Tenants");

            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_GeoRegions_GeoRegionId",
                table: "Tenants");

            migrationBuilder.DropTable(
                name: "Districts");

            migrationBuilder.DropTable(
                name: "GeoRegions");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_DistrictId",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_GeoRegionId",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_ManagerRegionAccesses_DistrictId",
                table: "ManagerRegionAccesses");

            migrationBuilder.DropIndex(
                name: "IX_ManagerRegionAccesses_GeoRegionId",
                table: "ManagerRegionAccesses");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "GeoRegionId",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "ManagerRegionAccesses");

            migrationBuilder.DropColumn(
                name: "GeoRegionId",
                table: "ManagerRegionAccesses");
        }
    }
}
