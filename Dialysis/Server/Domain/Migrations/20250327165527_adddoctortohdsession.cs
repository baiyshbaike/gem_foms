using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dialysis.Server.Domain.Migrations
{
    /// <inheritdoc />
    public partial class adddoctortohdsession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FioDepartmentHead",
                table: "HDSession",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FioDoctor",
                table: "HDSession",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FioDepartmentHead",
                table: "HDSession");

            migrationBuilder.DropColumn(
                name: "FioDoctor",
                table: "HDSession");
        }
    }
}
