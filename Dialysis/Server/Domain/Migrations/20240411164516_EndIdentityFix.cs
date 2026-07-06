using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dialysis.Server.Domain.Migrations
{
    /// <inheritdoc />
    public partial class EndIdentityFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Hd10Recommendations",
                table: "HDSession",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Hd1Hypotension",
                table: "HDSession",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Hd2Hypertension",
                table: "HDSession",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Hd3MuscleCrampsOfTheLimbs",
                table: "HDSession",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Hd4HeartRhythmDisturbances",
                table: "HDSession",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Hd5Headache",
                table: "HDSession",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Hd6AnginaAttacks",
                table: "HDSession",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Hd7Other",
                table: "HDSession",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Hd8CorrectionOfComplications",
                table: "HDSession",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Hd9PlannedAppointments",
                table: "HDSession",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hd10Recommendations",
                table: "HDSession");

            migrationBuilder.DropColumn(
                name: "Hd1Hypotension",
                table: "HDSession");

            migrationBuilder.DropColumn(
                name: "Hd2Hypertension",
                table: "HDSession");

            migrationBuilder.DropColumn(
                name: "Hd3MuscleCrampsOfTheLimbs",
                table: "HDSession");

            migrationBuilder.DropColumn(
                name: "Hd4HeartRhythmDisturbances",
                table: "HDSession");

            migrationBuilder.DropColumn(
                name: "Hd5Headache",
                table: "HDSession");

            migrationBuilder.DropColumn(
                name: "Hd6AnginaAttacks",
                table: "HDSession");

            migrationBuilder.DropColumn(
                name: "Hd7Other",
                table: "HDSession");

            migrationBuilder.DropColumn(
                name: "Hd8CorrectionOfComplications",
                table: "HDSession");

            migrationBuilder.DropColumn(
                name: "Hd9PlannedAppointments",
                table: "HDSession");
        }
    }
}
