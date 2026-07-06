using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dialysis.Server.Domain.Migrations
{
    /// <inheritdoc />
    public partial class modieiedExamPage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Ferrytip",
                table: "QualityExam1",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Fistul",
                table: "QualityExam1",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ParatGram",
                table: "QualityExam1",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ferrytip",
                table: "QualityExam1");

            migrationBuilder.DropColumn(
                name: "Fistul",
                table: "QualityExam1");

            migrationBuilder.DropColumn(
                name: "ParatGram",
                table: "QualityExam1");
        }
    }
}
