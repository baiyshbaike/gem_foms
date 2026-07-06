using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Dialysis.Server.Domain.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivePrice",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Price = table.Column<decimal>(type: "numeric", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivePrice", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Analysise",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: true),
                    AnalysisExt = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Note2 = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_Analysise", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisResult",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Inn = table.Column<string>(type: "text", nullable: true),
                    MedCardId = table.Column<long>(type: "bigint", nullable: false),
                    PatientId = table.Column<long>(type: "bigint", nullable: true),
                    AnalysisId = table.Column<long>(type: "bigint", nullable: false),
                    AnalysisResultGroupId = table.Column<long>(type: "bigint", nullable: true),
                    AnalysisName = table.Column<string>(type: "text", nullable: true),
                    Result = table.Column<double>(type: "double precision", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisResult", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisResultGroup",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Inn = table.Column<string>(type: "text", nullable: true),
                    PatientId = table.Column<long>(type: "bigint", nullable: true),
                    MedCardId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    UploadFile1 = table.Column<string>(type: "text", nullable: true),
                    UploadFile2 = table.Column<string>(type: "text", nullable: true),
                    UploadFile3 = table.Column<string>(type: "text", nullable: true),
                    UploadFile4 = table.Column<string>(type: "text", nullable: true),
                    UploadFile5 = table.Column<string>(type: "text", nullable: true),
                    UploadFile6 = table.Column<string>(type: "text", nullable: true),
                    UploadFile7 = table.Column<string>(type: "text", nullable: true),
                    UploadFile8 = table.Column<string>(type: "text", nullable: true),
                    UploadFile9 = table.Column<string>(type: "text", nullable: true),
                    UploadFile10 = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisResultGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "District",
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
                    table.PrimaryKey("PK_District", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Epicrisis",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PatientId = table.Column<long>(type: "bigint", nullable: false),
                    MedCardId = table.Column<long>(type: "bigint", nullable: false),
                    Inn = table.Column<string>(type: "text", nullable: true),
                    MedCardNumber = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    GlobalStatusId = table.Column<long>(type: "bigint", nullable: true),
                    ActFile = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Epicrisis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FirstAnalysis",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MedCardId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Ekg = table.Column<string>(type: "text", nullable: true),
                    EkgDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Eho = table.Column<string>(type: "text", nullable: true),
                    EhoDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Ogk = table.Column<string>(type: "text", nullable: true),
                    OgkDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Uzi = table.Column<string>(type: "text", nullable: true),
                    UziDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Uzi2 = table.Column<string>(type: "text", nullable: true),
                    UziDate2 = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Consulting = table.Column<string>(type: "text", nullable: true),
                    UploadFile1 = table.Column<string>(type: "text", nullable: true),
                    UploadFile2 = table.Column<string>(type: "text", nullable: true),
                    UploadFile3 = table.Column<string>(type: "text", nullable: true),
                    UploadFile4 = table.Column<string>(type: "text", nullable: true),
                    UploadFile5 = table.Column<string>(type: "text", nullable: true),
                    UploadFile6 = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirstAnalysis", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FirstAnalysisResult",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MedCardId = table.Column<long>(type: "bigint", nullable: false),
                    PatientId = table.Column<long>(type: "bigint", nullable: true),
                    AnalysisId = table.Column<long>(type: "bigint", nullable: false),
                    FirstAnalysisId = table.Column<long>(type: "bigint", nullable: true),
                    AnalysisName = table.Column<string>(type: "text", nullable: true),
                    Result = table.Column<double>(type: "double precision", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirstAnalysisResult", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FirstCardiovascular",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MedCardId = table.Column<long>(type: "bigint", nullable: false),
                    Perekardilnaia = table.Column<string>(type: "text", nullable: true),
                    Epigastralnaia = table.Column<string>(type: "text", nullable: true),
                    Vidimye = table.Column<string>(type: "text", nullable: true),
                    Prochee = table.Column<string>(type: "text", nullable: true),
                    Verhushechnaia = table.Column<string>(type: "text", nullable: true),
                    Drojanie = table.Column<string>(type: "text", nullable: true),
                    Prochee2 = table.Column<string>(type: "text", nullable: true),
                    Granitsa = table.Column<string>(type: "text", nullable: true),
                    Sosudistiy = table.Column<string>(type: "text", nullable: true),
                    Prochee3 = table.Column<string>(type: "text", nullable: true),
                    Ton = table.Column<string>(type: "text", nullable: true),
                    Shum = table.Column<string>(type: "text", nullable: true),
                    Prochee4 = table.Column<string>(type: "text", nullable: true),
                    Ad = table.Column<string>(type: "text", nullable: true),
                    AdChastota = table.Column<string>(type: "text", nullable: true),
                    AdRitm = table.Column<string>(type: "text", nullable: true),
                    AdDefisit = table.Column<string>(type: "text", nullable: true),
                    AdProchee = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirstCardiovascular", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FirstConfectionery",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MedCardId = table.Column<long>(type: "bigint", nullable: false),
                    Slizistaia = table.Column<string>(type: "text", nullable: true),
                    Polost = table.Column<string>(type: "text", nullable: true),
                    Yazyk = table.Column<string>(type: "text", nullable: true),
                    Zev = table.Column<string>(type: "text", nullable: true),
                    Osmotr = table.Column<string>(type: "text", nullable: true),
                    Jivot = table.Column<string>(type: "text", nullable: true),
                    Perkussia = table.Column<string>(type: "text", nullable: true),
                    Prochee = table.Column<string>(type: "text", nullable: true),
                    Palypatsia = table.Column<string>(type: "text", nullable: true),
                    Granitsa = table.Column<string>(type: "text", nullable: true),
                    Jelchniy = table.Column<string>(type: "text", nullable: true),
                    PodJelchniy = table.Column<string>(type: "text", nullable: true),
                    Selezenka = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirstConfectionery", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FirstEndocrine",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MedCardId = table.Column<long>(type: "bigint", nullable: false),
                    Shitovidnaia = table.Column<string>(type: "text", nullable: true),
                    Glaznaia = table.Column<string>(type: "text", nullable: true),
                    Potootdelenie = table.Column<string>(type: "text", nullable: true),
                    Prochee = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirstEndocrine", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FirstInspection",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MedCardId = table.Column<long>(type: "bigint", nullable: false),
                    Anamnez = table.Column<string>(type: "text", nullable: true),
                    AnamnezLive = table.Column<string>(type: "text", nullable: true),
                    Inn = table.Column<string>(type: "text", nullable: true),
                    Rost = table.Column<double>(type: "double precision", nullable: true),
                    Ves = table.Column<double>(type: "double precision", nullable: true),
                    Grudnoi = table.Column<string>(type: "text", nullable: true),
                    Jivot = table.Column<string>(type: "text", nullable: true),
                    Kozhnye = table.Column<string>(type: "text", nullable: true),
                    Tela = table.Column<string>(type: "text", nullable: true),
                    Svet = table.Column<string>(type: "text", nullable: true),
                    Suhost = table.Column<string>(type: "text", nullable: true),
                    Uprugost = table.Column<string>(type: "text", nullable: true),
                    Syp = table.Column<string>(type: "text", nullable: true),
                    Oteki = table.Column<string>(type: "text", nullable: true),
                    Limfaticheskiy = table.Column<string>(type: "text", nullable: true),
                    Deformatsia = table.Column<string>(type: "text", nullable: true),
                    Pripuhanie = table.Column<string>(type: "text", nullable: true),
                    Muskulatura = table.Column<string>(type: "text", nullable: true),
                    Boli = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirstInspection", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FirstNeuro",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MedCardId = table.Column<long>(type: "bigint", nullable: false),
                    Soznanye = table.Column<string>(type: "text", nullable: true),
                    Rech = table.Column<string>(type: "text", nullable: true),
                    Pohodka = table.Column<string>(type: "text", nullable: true),
                    Golovokrujenie = table.Column<string>(type: "text", nullable: true),
                    Litso = table.Column<string>(type: "text", nullable: true),
                    Glaznye = table.Column<string>(type: "text", nullable: true),
                    Zrachki = table.Column<string>(type: "text", nullable: true),
                    Dvijenie = table.Column<string>(type: "text", nullable: true),
                    Glotochnyi = table.Column<string>(type: "text", nullable: true),
                    Yazyk = table.Column<string>(type: "text", nullable: true),
                    Poverhnostnye = table.Column<string>(type: "text", nullable: true),
                    Potologicheskie = table.Column<string>(type: "text", nullable: true),
                    Date1 = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Sindrom = table.Column<string>(type: "text", nullable: true),
                    Meningealnye = table.Column<string>(type: "text", nullable: true),
                    Kerniga = table.Column<string>(type: "text", nullable: true),
                    Romberga = table.Column<string>(type: "text", nullable: true),
                    Date2 = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Intellect = table.Column<string>(type: "text", nullable: true),
                    Pamiat = table.Column<string>(type: "text", nullable: true),
                    Mnitelnost = table.Column<string>(type: "text", nullable: true),
                    Vnushaemost = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirstNeuro", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FirstRespiratory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MedCardId = table.Column<long>(type: "bigint", nullable: false),
                    Nosovoe = table.Column<string>(type: "text", nullable: true),
                    Grudnaia = table.Column<string>(type: "text", nullable: true),
                    Tip = table.Column<string>(type: "text", nullable: true),
                    Grudnoi = table.Column<string>(type: "text", nullable: true),
                    Glubina = table.Column<string>(type: "text", nullable: true),
                    Chastota = table.Column<string>(type: "text", nullable: true),
                    Golosovoe = table.Column<string>(type: "text", nullable: true),
                    Sravnitelnaia = table.Column<string>(type: "text", nullable: true),
                    Topograficheskaia = table.Column<string>(type: "text", nullable: true),
                    Dyhatelnye = table.Column<string>(type: "text", nullable: true),
                    Hrip = table.Column<string>(type: "text", nullable: true),
                    Prochee = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirstRespiratory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FirstUrogenital",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MedCardId = table.Column<long>(type: "bigint", nullable: false),
                    Chastota = table.Column<string>(type: "text", nullable: true),
                    Sutochnoe = table.Column<string>(type: "text", nullable: true),
                    Rezi = table.Column<string>(type: "text", nullable: true),
                    Nederjanie = table.Column<string>(type: "text", nullable: true),
                    SvetMochi = table.Column<string>(type: "text", nullable: true),
                    PochkiPalpatsia = table.Column<string>(type: "text", nullable: true),
                    PochkiSimptom = table.Column<string>(type: "text", nullable: true),
                    PochkiProchee = table.Column<string>(type: "text", nullable: true),
                    MochevoiPalpatsia = table.Column<string>(type: "text", nullable: true),
                    MochevoiPerkissia = table.Column<string>(type: "text", nullable: true),
                    MochevoiProchee = table.Column<string>(type: "text", nullable: true),
                    Prochee = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FirstUrogenital", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GlobalStatus",
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
                    table.PrimaryKey("PK_GlobalStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HDSession",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Inn = table.Column<string>(type: "text", nullable: true),
                    PatientId = table.Column<long>(type: "bigint", nullable: false),
                    MedCardId = table.Column<long>(type: "bigint", nullable: false),
                    MedCenterId = table.Column<long>(type: "bigint", nullable: false),
                    MachineId = table.Column<long>(type: "bigint", nullable: false),
                    Condition = table.Column<string>(type: "text", nullable: true),
                    Complaints = table.Column<string>(type: "text", nullable: true),
                    Program = table.Column<string>(type: "text", nullable: true),
                    Dialyzer = table.Column<string>(type: "text", nullable: true),
                    Correction = table.Column<string>(type: "text", nullable: true),
                    Reinfusion = table.Column<string>(type: "text", nullable: true),
                    Access = table.Column<string>(type: "text", nullable: true),
                    Anticoagulation = table.Column<string>(type: "text", nullable: true),
                    Uf = table.Column<string>(type: "text", nullable: true),
                    Speed = table.Column<string>(type: "text", nullable: true),
                    TypeDialyzer = table.Column<string>(type: "text", nullable: true),
                    Durators = table.Column<string>(type: "text", nullable: true),
                    StartWeight = table.Column<double>(type: "double precision", nullable: true),
                    EndWeight = table.Column<double>(type: "double precision", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SessionStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SessionEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    StatusId = table.Column<long>(type: "bigint", nullable: true),
                    TotalMinutes = table.Column<double>(type: "double precision", nullable: true),
                    TotalHours = table.Column<double>(type: "double precision", nullable: true),
                    ActivePrice = table.Column<decimal>(type: "numeric", nullable: true),
                    SetPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    Note2 = table.Column<string>(type: "text", nullable: true),
                    Note3 = table.Column<string>(type: "text", nullable: true),
                    ImageStart = table.Column<string>(type: "text", nullable: true),
                    ImageEnd = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HDSession", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HDSessionHour",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HDSessionId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Hour = table.Column<string>(type: "text", nullable: true),
                    Sys = table.Column<string>(type: "text", nullable: true),
                    Dia = table.Column<string>(type: "text", nullable: true),
                    Temp = table.Column<string>(type: "text", nullable: true),
                    Ritm = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HDSessionHour", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HDSessionPause",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HDSessionId = table.Column<long>(type: "bigint", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    PauseStart = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    PauseEnd = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TotalMinutes = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HDSessionPause", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MedCard",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PatientId = table.Column<long>(type: "bigint", nullable: false),
                    MedCenterId = table.Column<long>(type: "bigint", nullable: false),
                    Inn = table.Column<string>(type: "text", nullable: true),
                    MedCardNumber = table.Column<string>(type: "text", nullable: true),
                    M3 = table.Column<string>(type: "text", nullable: true),
                    DirectionDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ReceiptDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LeaveDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TotalHDSession = table.Column<int>(type: "integer", nullable: false),
                    Blood = table.Column<string>(type: "text", nullable: true),
                    Resus = table.Column<string>(type: "text", nullable: true),
                    AllergoAnamez = table.Column<string>(type: "text", nullable: true),
                    MainDiagnosis = table.Column<string>(type: "text", nullable: true),
                    Complication1 = table.Column<string>(type: "text", nullable: true),
                    Complication2 = table.Column<string>(type: "text", nullable: true),
                    Complication3 = table.Column<string>(type: "text", nullable: true),
                    Diagnosis1 = table.Column<string>(type: "text", nullable: true),
                    Diagnosis2 = table.Column<string>(type: "text", nullable: true),
                    Diagnosis3 = table.Column<string>(type: "text", nullable: true),
                    OtherAB = table.Column<string>(type: "text", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    OutcomeTreatment = table.Column<string>(type: "text", nullable: true),
                    Pedikulez = table.Column<string>(type: "text", nullable: true),
                    Chesotka = table.Column<string>(type: "text", nullable: true),
                    Vassermana = table.Column<string>(type: "text", nullable: true),
                    Fluorografia = table.Column<string>(type: "text", nullable: true),
                    Alchohol = table.Column<string>(type: "text", nullable: true),
                    Vgv = table.Column<string>(type: "text", nullable: true),
                    VgvDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Vgs = table.Column<string>(type: "text", nullable: true),
                    VgsDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Vich = table.Column<string>(type: "text", nullable: true),
                    VichDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Obosnovanie = table.Column<string>(type: "text", nullable: true),
                    Plan = table.Column<string>(type: "text", nullable: true),
                    IndividualPlan = table.Column<string>(type: "text", nullable: true),
                    Recommendation = table.Column<string>(type: "text", nullable: true),
                    DoctorId = table.Column<long>(type: "bigint", nullable: true),
                    DirectorId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    GlobalStatusId = table.Column<long>(type: "bigint", nullable: true),
                    ActFile = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedCard", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MedCenter",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Phone2 = table.Column<string>(type: "text", nullable: true),
                    DistrictId = table.Column<long>(type: "bigint", nullable: false),
                    RegionId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedCenter", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MedCenterMachine",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MedCenterId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Model = table.Column<string>(type: "text", nullable: true),
                    Number = table.Column<string>(type: "text", nullable: true),
                    Manufacturer = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true),
                    LicenseCountry = table.Column<string>(type: "text", nullable: true),
                    TotalSessions = table.Column<long>(type: "bigint", nullable: true),
                    CertificateNumber = table.Column<string>(type: "text", nullable: true),
                    CertificateCountry = table.Column<string>(type: "text", nullable: true),
                    CertificateMedCenterId = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedCenterMachine", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MedCenterUser",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MedCenterId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedCenterUser", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Patient",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Inn = table.Column<string>(type: "text", nullable: true),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    MiddleName = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Address2 = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Phone2 = table.Column<string>(type: "text", nullable: true),
                    DistrictId = table.Column<long>(type: "bigint", nullable: false),
                    RegionId = table.Column<long>(type: "bigint", nullable: false),
                    MedCenterId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    GroupId = table.Column<long>(type: "bigint", nullable: true),
                    StatusId = table.Column<long>(type: "bigint", nullable: true),
                    Gender = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    GroupActId = table.Column<long>(type: "bigint", nullable: true),
                    Image1 = table.Column<string>(type: "text", nullable: true),
                    Image2 = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patient", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PatientGroup",
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
                    table.PrimaryKey("PK_PatientGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PatientGroupAct",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SignFile = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    GroupId = table.Column<long>(type: "bigint", nullable: false),
                    PatientId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientGroupAct", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PatientStatus",
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
                    table.PrimaryKey("PK_PatientStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Region",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParentId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Region", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Status",
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
                    table.PrimaryKey("PK_Status", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    MIddleName = table.Column<string>(type: "text", nullable: true),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Password = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    ProfileId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastModifiedBy = table.Column<long>(type: "bigint", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastLogin = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastIp = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserProfile",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserProfileRole",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProfileId = table.Column<long>(type: "bigint", nullable: false),
                    RoleId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfileRole", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    RoleId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRole", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivePrice");

            migrationBuilder.DropTable(
                name: "Analysise");

            migrationBuilder.DropTable(
                name: "AnalysisResult");

            migrationBuilder.DropTable(
                name: "AnalysisResultGroup");

            migrationBuilder.DropTable(
                name: "District");

            migrationBuilder.DropTable(
                name: "Epicrisis");

            migrationBuilder.DropTable(
                name: "FirstAnalysis");

            migrationBuilder.DropTable(
                name: "FirstAnalysisResult");

            migrationBuilder.DropTable(
                name: "FirstCardiovascular");

            migrationBuilder.DropTable(
                name: "FirstConfectionery");

            migrationBuilder.DropTable(
                name: "FirstEndocrine");

            migrationBuilder.DropTable(
                name: "FirstInspection");

            migrationBuilder.DropTable(
                name: "FirstNeuro");

            migrationBuilder.DropTable(
                name: "FirstRespiratory");

            migrationBuilder.DropTable(
                name: "FirstUrogenital");

            migrationBuilder.DropTable(
                name: "GlobalStatus");

            migrationBuilder.DropTable(
                name: "HDSession");

            migrationBuilder.DropTable(
                name: "HDSessionHour");

            migrationBuilder.DropTable(
                name: "HDSessionPause");

            migrationBuilder.DropTable(
                name: "MedCard");

            migrationBuilder.DropTable(
                name: "MedCenter");

            migrationBuilder.DropTable(
                name: "MedCenterMachine");

            migrationBuilder.DropTable(
                name: "MedCenterUser");

            migrationBuilder.DropTable(
                name: "Patient");

            migrationBuilder.DropTable(
                name: "PatientGroup");

            migrationBuilder.DropTable(
                name: "PatientGroupAct");

            migrationBuilder.DropTable(
                name: "PatientStatus");

            migrationBuilder.DropTable(
                name: "Region");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "Status");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "UserProfile");

            migrationBuilder.DropTable(
                name: "UserProfileRole");

            migrationBuilder.DropTable(
                name: "UserRole");
        }
    }
}
