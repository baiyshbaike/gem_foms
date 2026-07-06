using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Dialysis.Server.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AktForms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "MedCenterMachine",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "Status",
                table: "MedCenterMachine",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "QualityExam1",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AktDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Organisation = table.Column<string>(type: "text", nullable: true),
                    Patient = table.Column<string>(type: "text", nullable: true),
                    PatientPin = table.Column<string>(type: "text", nullable: true),
                    FromDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ToDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    HD = table.Column<long>(type: "bigint", nullable: true),
                    HDF = table.Column<long>(type: "bigint", nullable: true),
                    UF = table.Column<long>(type: "bigint", nullable: true),
                    Lab = table.Column<bool>(type: "boolean", nullable: true),
                    Instrument = table.Column<bool>(type: "boolean", nullable: true),
                    Consulting = table.Column<bool>(type: "boolean", nullable: true),
                    After = table.Column<bool>(type: "boolean", nullable: true),
                    Iron = table.Column<bool>(type: "boolean", nullable: true),
                    Achieved = table.Column<long>(type: "bigint", nullable: true),
                    NotAchieved = table.Column<long>(type: "bigint", nullable: true),
                    TotalMonth = table.Column<long>(type: "bigint", nullable: true),
                    Dose = table.Column<bool>(type: "boolean", nullable: true),
                    Hb = table.Column<bool>(type: "boolean", nullable: true),
                    Fosfat = table.Column<bool>(type: "boolean", nullable: true),
                    Calcium = table.Column<bool>(type: "boolean", nullable: true),
                    Albumin = table.Column<bool>(type: "boolean", nullable: true),
                    Ad = table.Column<bool>(type: "boolean", nullable: true),
                    Document = table.Column<bool>(type: "boolean", nullable: true),
                    AllParams = table.Column<bool>(type: "boolean", nullable: true),
                    Conclusion = table.Column<string>(type: "text", nullable: true),
                    Recommendations = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    GlobalStatusId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityExam1", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QualityExam2",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AktDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Organisation = table.Column<string>(type: "text", nullable: true),
                    TuFoms = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    GlobalStatusId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityExam2", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QualityExam2Patient",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExamId = table.Column<long>(type: "bigint", nullable: false),
                    MedCardNo = table.Column<string>(type: "text", nullable: true),
                    PatientPin = table.Column<string>(type: "text", nullable: true),
                    SessionDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityExam2Patient", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QualityExam3",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AktDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    GlobalStatusId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityExam3", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QualityExam3Row",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExamId = table.Column<long>(type: "bigint", nullable: false),
                    Organisation = table.Column<string>(type: "text", nullable: true),
                    TotalMedCard = table.Column<long>(type: "bigint", nullable: true),
                    TotalSession = table.Column<long>(type: "bigint", nullable: true),
                    TotalDefects = table.Column<long>(type: "bigint", nullable: true),
                    TotalNotDefects = table.Column<long>(type: "bigint", nullable: true),
                    Defects = table.Column<long>(type: "bigint", nullable: true),
                    DefectsTotal = table.Column<long>(type: "bigint", nullable: true),
                    Defects1 = table.Column<long>(type: "bigint", nullable: true),
                    Defects2 = table.Column<long>(type: "bigint", nullable: true),
                    Defects3 = table.Column<long>(type: "bigint", nullable: true),
                    DefectTreatment = table.Column<long>(type: "bigint", nullable: true),
                    DefectTreatment1 = table.Column<long>(type: "bigint", nullable: true),
                    DefectTreatment2 = table.Column<long>(type: "bigint", nullable: true),
                    Extra1 = table.Column<string>(type: "text", nullable: true),
                    Extra2 = table.Column<string>(type: "text", nullable: true),
                    Extra3 = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityExam3Row", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QualityExam1");

            migrationBuilder.DropTable(
                name: "QualityExam2");

            migrationBuilder.DropTable(
                name: "QualityExam2Patient");

            migrationBuilder.DropTable(
                name: "QualityExam3");

            migrationBuilder.DropTable(
                name: "QualityExam3Row");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "MedCenterMachine");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "MedCenterMachine");
        }
    }
}
