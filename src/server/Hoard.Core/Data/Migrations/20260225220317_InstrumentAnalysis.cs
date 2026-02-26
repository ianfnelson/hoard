using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class InstrumentAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InstrumentAnalysis",
                columns: table => new
                {
                    InstrumentAnalysisId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InstrumentId = table.Column<int>(type: "int", nullable: false),
                    AsOfDate = table.Column<DateOnly>(type: "date", nullable: false),
                    FiftyTwoWeekHighClose = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    FiftyTwoWeekLowClose = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    ThirtyDayMedianVolume = table.Column<long>(type: "bigint", nullable: true),
                    FiftyTwoWeekVolatility = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2(3)", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstrumentAnalysis", x => x.InstrumentAnalysisId);
                    table.ForeignKey(
                        name: "FK_InstrumentAnalysis_Instrument_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instrument",
                        principalColumn: "InstrumentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentAnalysis_InstrumentId",
                table: "InstrumentAnalysis",
                column: "InstrumentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InstrumentAnalysis");
        }
    }
}
