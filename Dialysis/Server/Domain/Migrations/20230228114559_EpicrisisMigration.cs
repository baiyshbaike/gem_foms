using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dialysis.Server.Domain.Migrations
{
    /// <inheritdoc />
    public partial class EpicrisisMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Epicrisis",
                newName: "Uf");

            migrationBuilder.RenameColumn(
                name: "Note",
                table: "Epicrisis",
                newName: "URR");

            migrationBuilder.RenameColumn(
                name: "MedCardNumber",
                table: "Epicrisis",
                newName: "TrombABfist");

            migrationBuilder.RenameColumn(
                name: "Inn",
                table: "Epicrisis",
                newName: "Tromb");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Epicrisis",
                newName: "TotalHDF");

            migrationBuilder.AddColumn<string>(
                name: "Achtv",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ad",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Alt",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ast",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AzotAfter",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AzotDo",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bilirubin",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CaAfter",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CaDo",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateVgs",
                table: "Epicrisis",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateVgv",
                table: "Epicrisis",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Days",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Diagnosis",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "E",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Eritropoetin",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Erty",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fe",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fosfor",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gemotransfuzii",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gipertenzia",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gipotenzia",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GolovnoiBol",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GruppaKrovi",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ht",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Imt",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KAfter",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KDo",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KreatininAfter",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KreatininDo",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "L",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Leykty",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Lihorodka",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "M",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedHelpType",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MochevinaAfter",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MochevinaDo",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NaAfter",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NaDo",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ng",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObshiyBelok",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObshiyXC",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Osl",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Osn",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreparatFe",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Preparatov",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pti",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pya",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RazmerFiltra",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rw",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SaharKrovi",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Shok",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Soe",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sop",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartHeight",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartWeight",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Started",
                table: "Epicrisis",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sudorogi",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sya",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Toshnota",
                table: "Epicrisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TotalHD",
                table: "Epicrisis",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Achtv",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Ad",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Alt",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Ast",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "AzotAfter",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "AzotDo",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Bilirubin",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "CaAfter",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "CaDo",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "DateVgs",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "DateVgv",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Days",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Diagnosis",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "E",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Eritropoetin",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Erty",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Fe",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Fosfor",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Gemotransfuzii",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Gipertenzia",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Gipotenzia",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "GolovnoiBol",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "GruppaKrovi",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Ht",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Imt",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "KAfter",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "KDo",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "KreatininAfter",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "KreatininDo",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "L",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Leykty",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Lihorodka",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "M",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "MedHelpType",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "MochevinaAfter",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "MochevinaDo",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "NaAfter",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "NaDo",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Ng",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "ObshiyBelok",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "ObshiyXC",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Osl",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Osn",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "PreparatFe",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Preparatov",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Pti",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Pya",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "RazmerFiltra",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Rw",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "SaharKrovi",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Shok",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Soe",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Sop",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "StartHeight",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "StartWeight",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Started",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Sudorogi",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Sya",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "Toshnota",
                table: "Epicrisis");

            migrationBuilder.DropColumn(
                name: "TotalHD",
                table: "Epicrisis");

            migrationBuilder.RenameColumn(
                name: "Uf",
                table: "Epicrisis",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "URR",
                table: "Epicrisis",
                newName: "Note");

            migrationBuilder.RenameColumn(
                name: "TrombABfist",
                table: "Epicrisis",
                newName: "MedCardNumber");

            migrationBuilder.RenameColumn(
                name: "Tromb",
                table: "Epicrisis",
                newName: "Inn");

            migrationBuilder.RenameColumn(
                name: "TotalHDF",
                table: "Epicrisis",
                newName: "Description");
        }
    }
}
