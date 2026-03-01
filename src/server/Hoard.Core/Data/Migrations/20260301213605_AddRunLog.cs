using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRunLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RunType",
                columns: table => new
                {
                    RunTypeId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RunType", x => x.RunTypeId);
                });

            migrationBuilder.CreateTable(
                name: "RunLog",
                columns: table => new
                {
                    RunLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RunTypeId = table.Column<int>(type: "int", nullable: false),
                    RunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RunLog", x => x.RunLogId);
                    table.ForeignKey(
                        name: "FK_RunLog_RunType_RunTypeId",
                        column: x => x.RunTypeId,
                        principalTable: "RunType",
                        principalColumn: "RunTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RunLog_RunId",
                table: "RunLog",
                column: "RunId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RunLog_RunTypeId",
                table: "RunLog",
                column: "RunTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RunLog");

            migrationBuilder.DropTable(
                name: "RunType");
        }
    }
}
