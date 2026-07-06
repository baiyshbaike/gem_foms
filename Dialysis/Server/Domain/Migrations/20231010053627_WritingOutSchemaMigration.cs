using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Dialysis.Server.Domain.Migrations
{
    /// <inheritdoc />
    public partial class WritingOutSchemaMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UploadFile6",
                table: "FirstAnalysis",
                newName: "Recommend");

            migrationBuilder.RenameColumn(
                name: "UploadFile5",
                table: "FirstAnalysis",
                newName: "Planning");

            migrationBuilder.RenameColumn(
                name: "UploadFile4",
                table: "FirstAnalysis",
                newName: "Individual");

            migrationBuilder.RenameColumn(
                name: "UploadFile3",
                table: "FirstAnalysis",
                newName: "An9");

            migrationBuilder.RenameColumn(
                name: "UploadFile2",
                table: "FirstAnalysis",
                newName: "An8");

            migrationBuilder.RenameColumn(
                name: "UploadFile1",
                table: "FirstAnalysis",
                newName: "An7");

            migrationBuilder.AddColumn<string>(
                name: "Stul",
                table: "FirstConfectionery",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ambulator",
                table: "FirstAnalysis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "An1",
                table: "FirstAnalysis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "An10",
                table: "FirstAnalysis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "An10Date",
                table: "FirstAnalysis",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "An11",
                table: "FirstAnalysis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "An11Date",
                table: "FirstAnalysis",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "An12",
                table: "FirstAnalysis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "An12Date",
                table: "FirstAnalysis",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "An13",
                table: "FirstAnalysis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "An13Date",
                table: "FirstAnalysis",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "An14",
                table: "FirstAnalysis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "An14Date",
                table: "FirstAnalysis",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "An15",
                table: "FirstAnalysis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "An15Date",
                table: "FirstAnalysis",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "An16",
                table: "FirstAnalysis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "An16Date",
                table: "FirstAnalysis",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "An17",
                table: "FirstAnalysis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "An17Date",
                table: "FirstAnalysis",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "An18",
                table: "FirstAnalysis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "An18Date",
                table: "FirstAnalysis",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "An19",
                table: "FirstAnalysis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "An19Date",
                table: "FirstAnalysis",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "An1Date",
                table: "FirstAnalysis",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "An2",
                table: "FirstAnalysis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "An2Date",
                table: "FirstAnalysis",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "An3",
                table: "FirstAnalysis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "An3Date",
                table: "FirstAnalysis",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "An4",
                table: "FirstAnalysis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "An4Date",
                table: "FirstAnalysis",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "An5",
                table: "FirstAnalysis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "An5Date",
                table: "FirstAnalysis",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "An6",
                table: "FirstAnalysis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "An6Date",
                table: "FirstAnalysis",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "An7Date",
                table: "FirstAnalysis",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "An8Date",
                table: "FirstAnalysis",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "An9Date",
                table: "FirstAnalysis",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConsultingDate",
                table: "FirstAnalysis",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlanningDate",
                table: "FirstAnalysis",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FIODepartmentHead",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FIODoctor",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WritingOut",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PatientId = table.Column<long>(type: "bigint", nullable: false),
                    MedCenterId = table.Column<long>(type: "bigint", nullable: true),
                    MedCardId = table.Column<long>(type: "bigint", nullable: true),
                    Inn = table.Column<string>(type: "text", nullable: true),
                    MedCardNumber = table.Column<string>(type: "text", nullable: true),
                    PassportNum = table.Column<string>(type: "text", nullable: true),
                    Fio = table.Column<string>(type: "text", nullable: true),
                    FIODoctor = table.Column<string>(type: "text", nullable: true),
                    FIODepartmentHead = table.Column<string>(type: "text", nullable: true),
                    Familiarized = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    ReceiptDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LeaveDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TotalHDSession = table.Column<int>(type: "integer", nullable: false),
                    BloodResus = table.Column<string>(type: "text", nullable: true),
                    MainDiagnosis = table.Column<string>(type: "text", nullable: true),
                    SoputDiagnosis = table.Column<string>(type: "text", nullable: true),
                    Complication = table.Column<string>(type: "text", nullable: true),
                    Anamnez = table.Column<string>(type: "text", nullable: true),
                    AnamnezZabol = table.Column<string>(type: "text", nullable: true),
                    Allergo = table.Column<string>(type: "text", nullable: true),
                    Gemotrans = table.Column<string>(type: "text", nullable: true),
                    GepatitB = table.Column<string>(type: "text", nullable: true),
                    Sosud = table.Column<string>(type: "text", nullable: true),
                    Objectivus = table.Column<string>(type: "text", nullable: true),
                    AllResults = table.Column<string>(type: "text", nullable: true),
                    ExamFor = table.Column<string>(type: "text", nullable: true),
                    VirusBC = table.Column<string>(type: "text", nullable: true),
                    RW = table.Column<string>(type: "text", nullable: true),
                    Vich = table.Column<string>(type: "text", nullable: true),
                    Instrumental = table.Column<string>(type: "text", nullable: true),
                    Uzi = table.Column<string>(type: "text", nullable: true),
                    Ekg = table.Column<string>(type: "text", nullable: true),
                    Eho = table.Column<string>(type: "text", nullable: true),
                    Rentgen = table.Column<string>(type: "text", nullable: true),
                    Consulting = table.Column<string>(type: "text", nullable: true),
                    Otchet = table.Column<string>(type: "text", nullable: true),
                    TimeProcedure = table.Column<string>(type: "text", nullable: true),
                    Gemo = table.Column<string>(type: "text", nullable: true),
                    Sostoyania = table.Column<string>(type: "text", nullable: true),
                    Medikamentoz = table.Column<string>(type: "text", nullable: true),
                    Gospital = table.Column<string>(type: "text", nullable: true),
                    Recommend = table.Column<string>(type: "text", nullable: true),
                    DoctorId = table.Column<long>(type: "bigint", nullable: true),
                    DirectorId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    GlobalStatusId = table.Column<long>(type: "bigint", nullable: true),
                    ActFile = table.Column<string>(type: "text", nullable: true),
                    UniqId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WritingOut", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WritingOut");

            migrationBuilder.DropColumn(
                name: "Stul",
                table: "FirstConfectionery");

            migrationBuilder.DropColumn(
                name: "Ambulator",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An1",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An10",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An10Date",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An11",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An11Date",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An12",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An12Date",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An13",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An13Date",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An14",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An14Date",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An15",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An15Date",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An16",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An16Date",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An17",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An17Date",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An18",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An18Date",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An19",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An19Date",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An1Date",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An2",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An2Date",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An3",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An3Date",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An4",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An4Date",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An5",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An5Date",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An6",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An6Date",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An7Date",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An8Date",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "An9Date",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "ConsultingDate",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "PlanningDate",
                table: "FirstAnalysis");

            migrationBuilder.DropColumn(
                name: "FIODepartmentHead",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "FIODoctor",
                table: "Epicrisis");

            migrationBuilder.RenameColumn(
                name: "Recommend",
                table: "FirstAnalysis",
                newName: "UploadFile6");

            migrationBuilder.RenameColumn(
                name: "Planning",
                table: "FirstAnalysis",
                newName: "UploadFile5");

            migrationBuilder.RenameColumn(
                name: "Individual",
                table: "FirstAnalysis",
                newName: "UploadFile4");

            migrationBuilder.RenameColumn(
                name: "An9",
                table: "FirstAnalysis",
                newName: "UploadFile3");

            migrationBuilder.RenameColumn(
                name: "An8",
                table: "FirstAnalysis",
                newName: "UploadFile2");

            migrationBuilder.RenameColumn(
                name: "An7",
                table: "FirstAnalysis",
                newName: "UploadFile1");
        }
    }
}
