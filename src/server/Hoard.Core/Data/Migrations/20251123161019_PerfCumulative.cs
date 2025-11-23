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
            migrationBuilder.DropTable(
                name: "Cashflow");

            migrationBuilder.CreateTable(
                name: "PortfolioPerformanceCumulative",
                columns: table => new
                {
                    PortfolioPerformanceCumulativeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PortfolioId = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreviousValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValueChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnrealisedGain = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RealisedGain = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Income = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Return1D = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return1W = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return1M = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return3M = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return6M = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return1Y = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return3Y = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return5Y = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ReturnYtd = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ReturnAllTime = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    AnnualisedReturn = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioPerformanceCumulative", x => x.PortfolioPerformanceCumulativeId);
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
                    PortfolioPerformanceCumulativeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    PortfolioWeightPercent = table.Column<decimal>(type: "decimal(9,4)", nullable: false),
                    CostBasis = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Units = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PreviousValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValueChange = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnrealisedGain = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RealisedGain = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Income = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Return1D = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return1W = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return1M = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return3M = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return6M = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return1Y = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return3Y = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    Return5Y = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ReturnYtd = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    ReturnAllTime = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    AnnualisedReturn = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    UpdatedUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PositionPerformanceCumulative", x => x.PortfolioPerformanceCumulativeId);
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
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PositionPerformanceCumulative_PositionId",
                table: "PositionPerformanceCumulative",
                column: "PositionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortfolioPerformanceCumulative");

            migrationBuilder.DropTable(
                name: "PositionPerformanceCumulative");

            migrationBuilder.CreateTable(
                name: "Cashflow",
                columns: table => new
                {
                    CashflowId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    InstrumentId = table.Column<int>(type: "int", nullable: true),
                    TransactionId = table.Column<int>(type: "int", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2(3)", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cashflow", x => x.CashflowId);
                    table.ForeignKey(
                        name: "FK_Cashflow_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cashflow_Instrument_InstrumentId",
                        column: x => x.InstrumentId,
                        principalTable: "Instrument",
                        principalColumn: "InstrumentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cashflow_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cashflow_AccountId_Date",
                table: "Cashflow",
                columns: new[] { "AccountId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Cashflow_InstrumentId_Date",
                table: "Cashflow",
                columns: new[] { "InstrumentId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Cashflow_TransactionId",
                table: "Cashflow",
                column: "TransactionId",
                unique: true);
        }
    }
}
