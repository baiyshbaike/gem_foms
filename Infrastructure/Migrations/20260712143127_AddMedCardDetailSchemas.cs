using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMedCardDetailSchemas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MedCardApprovals",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MedCardId = table.Column<long>(type: "bigint", nullable: false),
                    DoctorId = table.Column<long>(type: "bigint", nullable: true),
                    DirectorId = table.Column<long>(type: "bigint", nullable: true),
                    DoctorNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DirectorNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DoctorSignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DirectorSignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PatientFamiliarizedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedCardApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedCardApprovals_MedCards_MedCardId",
                        column: x => x.MedCardId,
                        principalTable: "MedCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MedCardClinicalInfos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MedCardId = table.Column<long>(type: "bigint", nullable: false),
                    BloodGroup = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    RhFactor = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AllergyHistory = table.Column<string>(type: "text", nullable: true),
                    OutTreatment = table.Column<string>(type: "text", nullable: true),
                    Justification = table.Column<string>(type: "text", nullable: true),
                    Plan = table.Column<string>(type: "text", nullable: true),
                    IndividualPlan = table.Column<string>(type: "text", nullable: true),
                    Recommendation = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedCardClinicalInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedCardClinicalInfos_MedCards_MedCardId",
                        column: x => x.MedCardId,
                        principalTable: "MedCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MedCardDiagnoses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MedCardId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DiagnosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedCardDiagnoses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedCardDiagnoses_MedCards_MedCardId",
                        column: x => x.MedCardId,
                        principalTable: "MedCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MedCardInfectionScreenings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MedCardId = table.Column<long>(type: "bigint", nullable: false),
                    Pediculosis = table.Column<int>(type: "integer", nullable: false),
                    Scabies = table.Column<int>(type: "integer", nullable: false),
                    Wasserman = table.Column<int>(type: "integer", nullable: false),
                    Fluorography = table.Column<int>(type: "integer", nullable: false),
                    AlcoholUse = table.Column<int>(type: "integer", nullable: false),
                    HepatitisB = table.Column<int>(type: "integer", nullable: false),
                    HepatitisBCheckedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    HepatitisC = table.Column<int>(type: "integer", nullable: false),
                    HepatitisCCheckedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Hiv = table.Column<int>(type: "integer", nullable: false),
                    HivCheckedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedCardInfectionScreenings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedCardInfectionScreenings_MedCards_MedCardId",
                        column: x => x.MedCardId,
                        principalTable: "MedCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedCardApprovals_CreatedAt",
                table: "MedCardApprovals",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MedCardApprovals_MedCardId",
                table: "MedCardApprovals",
                column: "MedCardId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedCardApprovals_TenantId",
                table: "MedCardApprovals",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_MedCardClinicalInfos_CreatedAt",
                table: "MedCardClinicalInfos",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MedCardClinicalInfos_MedCardId",
                table: "MedCardClinicalInfos",
                column: "MedCardId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedCardClinicalInfos_TenantId",
                table: "MedCardClinicalInfos",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_MedCardDiagnoses_CreatedAt",
                table: "MedCardDiagnoses",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MedCardDiagnoses_MedCardId_Type_SortOrder",
                table: "MedCardDiagnoses",
                columns: new[] { "MedCardId", "Type", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MedCardDiagnoses_TenantId",
                table: "MedCardDiagnoses",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_MedCardInfectionScreenings_CreatedAt",
                table: "MedCardInfectionScreenings",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MedCardInfectionScreenings_MedCardId",
                table: "MedCardInfectionScreenings",
                column: "MedCardId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedCardInfectionScreenings_TenantId",
                table: "MedCardInfectionScreenings",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedCardApprovals");

            migrationBuilder.DropTable(
                name: "MedCardClinicalInfos");

            migrationBuilder.DropTable(
                name: "MedCardDiagnoses");

            migrationBuilder.DropTable(
                name: "MedCardInfectionScreenings");
        }
    }
}
