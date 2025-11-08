using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class Valuations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InstrumentValuation");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "PortfolioValuation",
                newName: "PortfolioValuationId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "AccountValuation",
                newName: "AccountValuationId");

            migrationBuilder.CreateTable(
                name: "HoldingValuation",
                columns: table => new
                {
                    HoldingValuationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoldingId = table.Column<int>(type: "int", nullable: false),
                    ValuationGbp = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoldingValuation", x => x.HoldingValuationId);
                    table.ForeignKey(
                        name: "FK_HoldingValuation_Holding_HoldingId",
                        column: x => x.HoldingId,
                        principalTable: "Holding",
                        principalColumn: "HoldingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HoldingValuation_HoldingId",
                table: "HoldingValuation",
                column: "HoldingId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HoldingValuation");

            migrationBuilder.RenameColumn(
                name: "PortfolioValuationId",
                table: "PortfolioValuation",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "AccountValuationId",
                table: "AccountValuation",
                newName: "Id");

            migrationBuilder.CreateTable(
                name: "InstrumentValuation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstrumentId = table.Column<int>(type: "int", nullable: false),
                    AsOfDate = table.Column<DateOnly>(type: "date", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ValuationGbp = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstrumentValuation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstrumentValuation_Instrument_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instrument",
                        principalColumn: "InstrumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentValuation_InstrumentId_AsOfDate",
                table: "InstrumentValuation",
                columns: new[] { "InstrumentId", "AsOfDate" },
                unique: true);
        }
    }
}
