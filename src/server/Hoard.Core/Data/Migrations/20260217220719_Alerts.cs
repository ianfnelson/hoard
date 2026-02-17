using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class Alerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertType",
                columns: table => new
                {
                    AlertTypeId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertType", x => x.AlertTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Alert",
                columns: table => new
                {
                    AlertId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlertTypeId = table.Column<int>(type: "int", nullable: false),
                    TriggeredDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false),
                    InstrumentId = table.Column<int>(type: "int", nullable: true),
                    PortfolioId = table.Column<int>(type: "int", nullable: true),
                    SuppressionKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UrlTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alert", x => x.AlertId);
                    table.ForeignKey(
                        name: "FK_Alert_AlertType_AlertTypeId",
                        column: x => x.AlertTypeId,
                        principalTable: "AlertType",
                        principalColumn: "AlertTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Alert_Instrument_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instrument",
                        principalColumn: "InstrumentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Alert_Portfolio_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolio",
                        principalColumn: "PortfolioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alert_AlertTypeId",
                table: "Alert",
                column: "AlertTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Alert_CreatedUtc",
                table: "Alert",
                column: "CreatedUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Alert_InstrumentId",
                table: "Alert",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Alert_PortfolioId",
                table: "Alert",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_Alert_SuppressionKey",
                table: "Alert",
                column: "SuppressionKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alert");

            migrationBuilder.DropTable(
                name: "AlertType");
        }
    }
}
