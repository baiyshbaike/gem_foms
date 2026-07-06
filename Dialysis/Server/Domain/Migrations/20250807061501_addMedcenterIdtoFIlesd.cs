using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dialysis.Server.Domain.Migrations
{
    /// <inheritdoc />
    public partial class addMedcenterIdtoFIlesd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "MedCenterId",
                table: "MedCenterPatientFiles",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UploadedByUserId",
                table: "MedCenterPatientFiles",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MedCenterPatientFiles_MedCenterId",
                table: "MedCenterPatientFiles",
                column: "MedCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_MedCenterPatientFiles_UploadedByUserId",
                table: "MedCenterPatientFiles",
                column: "UploadedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MedCenterPatientFiles_MedCenter_MedCenterId",
                table: "MedCenterPatientFiles",
                column: "MedCenterId",
                principalTable: "MedCenter",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MedCenterPatientFiles_User_UploadedByUserId",
                table: "MedCenterPatientFiles",
                column: "UploadedByUserId",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedCenterPatientFiles_MedCenter_MedCenterId",
                table: "MedCenterPatientFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_MedCenterPatientFiles_User_UploadedByUserId",
                table: "MedCenterPatientFiles");

            migrationBuilder.DropIndex(
                name: "IX_MedCenterPatientFiles_MedCenterId",
                table: "MedCenterPatientFiles");

            migrationBuilder.DropIndex(
                name: "IX_MedCenterPatientFiles_UploadedByUserId",
                table: "MedCenterPatientFiles");

            migrationBuilder.DropColumn(
                name: "MedCenterId",
                table: "MedCenterPatientFiles");

            migrationBuilder.DropColumn(
                name: "UploadedByUserId",
                table: "MedCenterPatientFiles");
        }
    }
}
