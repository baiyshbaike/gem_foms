using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dialysis.Server.Domain.Migrations
{
    /// <inheritdoc />
    public partial class patienthistoryid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PatientHistoryId",
                table: "MedCenterPatientFiles",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedCenterPatientFiles_PatientHistoryId",
                table: "MedCenterPatientFiles",
                column: "PatientHistoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedCenterPatientFiles_PatientHistory_PatientHistoryId",
                table: "MedCenterPatientFiles",
                column: "PatientHistoryId",
                principalTable: "PatientHistory",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedCenterPatientFiles_PatientHistory_PatientHistoryId",
                table: "MedCenterPatientFiles");

            migrationBuilder.DropIndex(
                name: "IX_MedCenterPatientFiles_PatientHistoryId",
                table: "MedCenterPatientFiles");

            migrationBuilder.DropColumn(
                name: "PatientHistoryId",
                table: "MedCenterPatientFiles");
        }
    }
}
