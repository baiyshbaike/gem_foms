using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dialysis.Server.Domain.Migrations
{
    /// <inheritdoc />
    public partial class tundukcashsessions2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TundukCashSessions_HDSession_HdSessionId",
                table: "TundukCashSessions");

            migrationBuilder.DropIndex(
                name: "IX_TundukCashSessions_HdSessionId",
                table: "TundukCashSessions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TundukCashSessions_HdSessionId",
                table: "TundukCashSessions",
                column: "HdSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_TundukCashSessions_HDSession_HdSessionId",
                table: "TundukCashSessions",
                column: "HdSessionId",
                principalTable: "HDSession",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
