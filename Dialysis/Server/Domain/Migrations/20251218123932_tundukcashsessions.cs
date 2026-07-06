using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dialysis.Server.Domain.Migrations
{
    /// <inheritdoc />
    public partial class tundukcashsessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TundukCashSessions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    HdSessionId = table.Column<long>(type: "bigint", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TundukCashSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TundukCashSessions_HDSession_HdSessionId",
                        column: x => x.HdSessionId,
                        principalTable: "HDSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TundukCashSessions_HdSessionId",
                table: "TundukCashSessions",
                column: "HdSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TundukCashSessions");
        }
    }
}
