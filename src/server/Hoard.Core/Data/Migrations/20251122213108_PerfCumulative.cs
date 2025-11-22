using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hoard.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class PerfCumulative : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "PerformanceCumulativeSequence");

            migrationBuilder.CreateTable(
                name: "PortfolioPerformanceCumulative",
                columns: table => new
                {
                    PerformanceCumulativeId = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [PerformanceCumulativeSequence]"),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreviousValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValueChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValueChangePercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnrealisedGain = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RealisedGain = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Income = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Return1W = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Return1M = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Return3M = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Return6M = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Return1Y = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Return3Y = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Return5Y = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReturnYtd = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReturnAllTime = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AnnualisedReturn = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PortfolioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioPerformanceCumulative", x => x.PerformanceCumulativeId);
                    table.ForeignKey(
                        name: "FK_PortfolioPerformanceCumulative_Portfolio_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolio",
                        principalColumn: "PortfolioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PositionPerformanceCumulative",
                columns: table => new
                {
                    PerformanceCumulativeId = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR [PerformanceCumulativeSequence]"),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreviousValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValueChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValueChangePercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnrealisedGain = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RealisedGain = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Income = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Return1W = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Return1M = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Return3M = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Return6M = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Return1Y = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Return3Y = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Return5Y = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReturnYtd = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReturnAllTime = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AnnualisedReturn = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    PortfolioWeightPercent = table.Column<decimal>(type: "decimal(9,4)", nullable: false),
                    CostBasis = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Units = table.Column<decimal>(type: "decimal(18,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PositionPerformanceCumulative", x => x.PerformanceCumulativeId);
                    table.ForeignKey(
                        name: "FK_PositionPerformanceCumulative_Position_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Position",
                        principalColumn: "PositionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioPerformanceCumulative_PortfolioId",
                table: "PortfolioPerformanceCumulative",
                column: "PortfolioId",
                unique: true,
                filter: "[PortfolioId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PositionPerformanceCumulative_PositionId",
                table: "PositionPerformanceCumulative",
                column: "PositionId",
                unique: true,
                filter: "[PositionId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortfolioPerformanceCumulative");

            migrationBuilder.DropTable(
                name: "PositionPerformanceCumulative");

            migrationBuilder.DropSequence(
                name: "PerformanceCumulativeSequence");
        }
    }
}
